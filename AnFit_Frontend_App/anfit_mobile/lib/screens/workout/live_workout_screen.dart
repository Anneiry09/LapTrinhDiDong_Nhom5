import 'dart:async';
import 'dart:math';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_blue_plus/flutter_blue_plus.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:fl_chart/fl_chart.dart';
import '../../core/theme/app_colors.dart';
import '../../providers/service_providers.dart';
import '../../widgets/workout/workout_video_player.dart';
import '../../widgets/anfit_toast.dart';

/// ═══════════════════════════════════════════════════════════
/// LiveWorkoutScreen — Màn hình Tập luyện & Kết nối Bluetooth Nhịp tim Real-time
/// ═══════════════════════════════════════════════════════════
class LiveWorkoutScreen extends ConsumerStatefulWidget {
  final Map<String, dynamic> plan;
  final int dayIndex;

  const LiveWorkoutScreen({
    super.key,
    required this.plan,
    required this.dayIndex,
  });

  @override
  ConsumerState<LiveWorkoutScreen> createState() => _LiveWorkoutScreenState();
}

class _LiveWorkoutScreenState extends ConsumerState<LiveWorkoutScreen> with TickerProviderStateMixin {
  // Trạng thái quét & kết nối
  bool _isBluetoothOn = false;
  bool _isScanning = false;
  List<ScanResult> _scanResults = [];
  BluetoothDevice? _targetDevice;
  BluetoothConnectionState _connectionState = BluetoothConnectionState.disconnected;
  bool _isConnecting = false;
  String _connectionStatusMessage = 'Đang đồng bộ cổng GATT Services & đặc tính Nhịp tim...';

  // Luồng dữ liệu và Đồng hồ tập luyện
  StreamSubscription? _adapterStateSub;
  StreamSubscription? _scanResultsSub;
  StreamSubscription? _isScanningSub;
  StreamSubscription? _connStateSub;
  StreamSubscription? _hrSub;

  Timer? _workoutTimer;
  int _secondsElapsed = 0;
  List<int> _heartRateHistory = [];
  int _currentHeartRate = 0;
  int _packetCount = 0;
  double _caloriesBurned = 0.0;

  // Thông số người dùng phục vụ tính Calories (Keytel Formula)
  double _userWeight = 70.0;
  int _userAge = 25;
  bool _userIsMale = true;

  // Quản lý danh sách bài tập hiện tại
  int _currentExerciseIndex = 0;
  late List _activities;

  // Controller hiệu ứng đập nhịp tim động
  late AnimationController _pulseController;

  @override
  void initState() {
    super.initState();
    _activities = widget.plan['workoutActivities'] as List? ?? [];
    
    _pulseController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1000),
    )..repeat(reverse: true);

    _initBluetooth();
    _loadUserProfile();
  }

  @override
  void dispose() {
    _workoutTimer?.cancel();
    _adapterStateSub?.cancel();
    _scanResultsSub?.cancel();
    _isScanningSub?.cancel();
    _connStateSub?.cancel();
    _hrSub?.cancel();
    _pulseController.dispose();
    
    // Ngắt kết nối thiết bị khi thoát
    ref.read(bleHeartRateServiceProvider).disconnectDevice();
    super.dispose();
  }

  /// Tải thông tin người dùng từ server để tính toán calo tối ưu
  Future<void> _loadUserProfile() async {
    try {
      final profile = await ref.read(userServiceProvider).getProfile();
      setState(() {
        _userWeight = (profile['weight'] ?? 70.0).toDouble();
        _userAge = profile['age'] ?? 25;
        _userIsMale = (profile['gender'] ?? 1) == 1;
      });
    } catch (_) {
      // Fallback giữ nguyên mặc định
    }
  }

  /// Khởi tạo Bluetooth và quản lý trạng thái adapter
  void _initBluetooth() {
    _adapterStateSub = FlutterBluePlus.adapterState.listen((state) {
      setState(() {
        _isBluetoothOn = state == BluetoothAdapterState.on;
      });
      if (_isBluetoothOn) {
        _startScan();
      }
    });

    // Lắng nghe kết quả quét thiết bị
    _scanResultsSub = FlutterBluePlus.scanResults.listen((results) {
      setState(() {
        _scanResults = results;
      });
    });

    // Lắng nghe trạng thái đang quét
    _isScanningSub = FlutterBluePlus.isScanning.listen((scanning) {
      setState(() {
        _isScanning = scanning;
      });
    });
  }

  /// Bắt đầu quét BLE tìm đồng hồ đo nhịp tim
  void _startScan() async {
    setState(() {
      _scanResults = [];
    });
    try {
      await ref.read(bleHeartRateServiceProvider).startScan();
    } catch (e) {
      if (mounted) {
        AnFitToast.show(context, e.toString(), isError: true);
      }
    }
  }

  /// Kết nối vào đồng hồ của người dùng
  Future<void> _connectToDevice(BluetoothDevice device) async {
    setState(() {
      _targetDevice = device;
      _isConnecting = true;
      _connectionStatusMessage = 'Đang khởi động tiến trình kết nối...';
    });

    try {
      final bleService = ref.read(bleHeartRateServiceProvider);

      // Giải phóng subscription cũ nếu đang có để tránh bị duplicate luồng
      await _connStateSub?.cancel();
      _connStateSub = null;
      await _hrSub?.cancel();
      _hrSub = null;

      // 1. Đăng ký lắng nghe trạng thái kết nối thực tế TRƯỚC khi gọi kết nối để tránh mất event
      _connStateSub = bleService.connectionStateStream.listen((state) {
        if (!mounted) return;
        setState(() {
          _connectionState = state;
        });
        if (state == BluetoothConnectionState.disconnected) {
          _stopWorkoutTimer();
        }
      });

      // 2. Đăng ký nhận nhịp tim thời gian thực TRƯỚC
      _hrSub = bleService.heartRateStream.listen((hr) {
        if (!mounted) return;
        setState(() {
          _currentHeartRate = hr;
          _packetCount++;
          _heartRateHistory.add(hr);
          if (_heartRateHistory.length > 50) {
            _heartRateHistory.removeAt(0); // Giữ tối đa 50 điểm vẽ đồ thị
          }
          
          // Thay đổi tốc độ đập của vòng tròn nhịp tim tương thích với nhịp thực
          _updatePulseDuration(hr);
        });
      });

      // 3. Thực hiện gọi kết nối bất đồng bộ với callback cập nhật trạng thái chi tiết
      await bleService.connectToDevice(device, onProgress: (msg) {
        setState(() {
          _connectionStatusMessage = msg;
        });
      });

      // 4. Nếu kết nối thành công và không bắn ra Exception, cập nhật trạng thái UI và bắt đầu đếm giờ tập!
      setState(() {
        _isConnecting = false;
        _connectionState = BluetoothConnectionState.connected;
      });
      _startWorkout();

    } catch (e) {
      setState(() {
        _isConnecting = false;
        _connectionState = BluetoothConnectionState.disconnected;
        _targetDevice = null;
      });
      
      // Hiển thị hộp thoại hướng dẫn chi tiết khi gặp lỗi kết nối BLE
      _showConnectionErrorDialog(e.toString());
    }
  }

  /// Cập nhật tốc độ hiệu ứng đập tim động (duration = 60000ms / bpm)
  void _updatePulseDuration(int bpm) {
    if (bpm <= 0) return;
    final ms = (60000 / bpm).round();
    // Giới hạn tốc độ trong khoảng thực tế 300ms -> 1500ms
    final clampedMs = ms.clamp(300, 1500);
    _pulseController.duration = Duration(milliseconds: clampedMs);
    if (!_pulseController.isAnimating) {
      _pulseController.repeat(reverse: true);
    }
  }

  /// Tính toán lượng Calorie đốt cháy theo công thức Keytel (JSS, 2005)
  void _updateCalories(int hr) {
    if (hr <= 0) return;
    
    // Tính số phút trôi qua (1 giây = 1/60 phút)
    double timeMinutes = 1 / 60;
    double cal = 0.0;

    if (_userIsMale) {
      cal = ((-55.0969 + (0.6309 * hr) + (0.1988 * _userWeight) + (0.2017 * _userAge)) / 4.184) * timeMinutes;
    } else {
      cal = ((-20.4022 + (0.4472 * hr) - (0.1263 * _userWeight) + (0.0740 * _userAge)) / 4.184) * timeMinutes;
    }

    _caloriesBurned += max(0.0, cal);
  }

  /// Bắt đầu bấm giờ buổi tập
  void _startWorkout() {
    _workoutTimer?.cancel();
    _secondsElapsed = 0;
    _packetCount = 0;
    _caloriesBurned = 0.0;
    _heartRateHistory = [];
    _workoutTimer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (!mounted) return;
      setState(() {
        _secondsElapsed++;
        if (_currentHeartRate > 0) {
          _updateCalories(_currentHeartRate);
        }
      });
    });
  }

  /// Dừng bấm giờ
  void _stopWorkoutTimer() {
    _workoutTimer?.cancel();
    _workoutTimer = null;
  }

  /// Định dạng giây sang dạng mm:ss
  String _formatDuration(int seconds) {
    final m = (seconds ~/ 60).toString().padLeft(2, '0');
    final s = (seconds % 60).toString().padLeft(2, '0');
    return '$m:$s';
  }

  /// Tính nhịp tim trung bình
  int get _averageHeartRate {
    if (_heartRateHistory.isEmpty) return 0;
    return (_heartRateHistory.reduce((a, b) => a + b) / _heartRateHistory.length).round();
  }

  /// Tính nhịp tim tối đa
  int get _maxHeartRate {
    if (_heartRateHistory.isEmpty) return 0;
    return _heartRateHistory.reduce(max);
  }

  /// Hoàn thành buổi tập, đẩy dữ liệu thực tế lên C# Backend & SQL Server
  Future<void> _finishWorkout() async {
    final planDate = widget.plan['planDate']?.toString().substring(0, 10) ?? '';
    final todayStr = DateTime.now().toIso8601String().substring(0, 10);
    final isPast = planDate.compareTo(todayStr) < 0;
    final isMissed = isPast && (widget.plan['status'] as int? ?? 1) != 2;

    if (isMissed) {
      _stopWorkoutTimer();
      ref.read(bleHeartRateServiceProvider).disconnectDevice();
      showDialog(
        context: context,
        barrierDismissible: false,
        builder: (ctx) => AlertDialog(
          backgroundColor: Theme.of(context).brightness == Brightness.dark ? AppColors.darkSurface : AppColors.lightSurface,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(24)),
          title: Row(
            children: [
              Icon(Icons.warning_amber_rounded, color: AppColors.error, size: 28),
              const SizedBox(width: 10),
              Expanded(
                child: Text(
                  'Không Thể Hoàn Thành!',
                  style: GoogleFonts.outfit(fontWeight: FontWeight.bold, fontSize: 18),
                ),
              ),
            ],
          ),
          content: Text(
            'Lịch tập ngày ${widget.dayIndex} đã bị bỏ lỡ và không thể lưu kết quả hoàn thành. Bạn có thể tiếp tục tập luyện để rèn luyện sức khỏe.',
            style: GoogleFonts.inter(fontSize: 14, height: 1.5),
          ),
          actions: [
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: () {
                  Navigator.pop(ctx); // Close dialog
                  Navigator.pop(context); // Exit LiveWorkoutScreen
                },
                style: ElevatedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 12),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                ),
                child: const Text('Đóng'),
              ),
            ),
          ],
        ),
      );
      return;
    }

    _stopWorkoutTimer();
    
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (ctx) => const Center(
        child: CircularProgressIndicator(color: AppColors.auroraCyan),
      ),
    );

    try {
      final planId = widget.plan['id'];
      
      // 1. Đánh dấu hoàn thành trên WorkoutPlan của C#
      await ref.read(workoutServiceProvider).updateStatus(planId, status: 2);
      
      // 2. Ghi nhận nhịp tim và calo thực tế đo được từ đồng hồ lên DailyHealthLogs
      final todayStr = DateTime.now().toIso8601String().substring(0, 10);
      final payload = {
        'totalSteps': 0, // Buổi tập không đồng bộ steps
        'activeCalories': double.parse(_caloriesBurned.toStringAsFixed(1)),
        'restingHeartRate': 60, // Nhịp nghỉ ước lượng
        'maxHeartRate': _maxHeartRate,
        'sleepDurationMinutes': 0,
        'stressLevel': 0.0,
        'targetDate': todayStr,
      };

      await ref.read(apiClientProvider).dio.post(
        '/DailyHealthLogs/sync-health-connect',
        data: payload,
      );

      if (mounted) {
        Navigator.pop(context); // Tắt loading dialog
        _showSuccessDialog();
      }

    } catch (e) {
      if (mounted) {
        Navigator.pop(context); // Tắt loading dialog
        AnFitToast.show(context, 'Lỗi khi lưu kết quả: $e', isError: true);
      }
    }
  }

  /// Hiển thị màn hình tổng kết hoành tráng
  void _showSuccessDialog() {
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (ctx) {
        final isDark = Theme.of(context).brightness == Brightness.dark;
        return AlertDialog(
          backgroundColor: isDark ? AppColors.darkSurface : AppColors.lightSurface,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(24)),
          title: Center(
            child: Column(children: [
              const Icon(Icons.stars_rounded, color: AppColors.success, size: 54),
              const SizedBox(height: 12),
              Text(
                'Chúc Mừng Bạn!',
                style: GoogleFonts.outfit(fontSize: 22, fontWeight: FontWeight.bold),
              ),
            ]),
          ),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(
                'Bạn đã hoàn thành xuất sắc ngày tập $widget.dayIndex với nhịp tim thực tế từ đồng hồ Garmin Forerunner 55!',
                textAlign: TextAlign.center,
                style: GoogleFonts.inter(fontSize: 14, height: 1.6),
              ),
              const SizedBox(height: 20),
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: isDark ? AppColors.darkCard : AppColors.lightCard,
                  borderRadius: BorderRadius.circular(16),
                ),
                child: Column(children: [
                  _buildSummaryRow('Thời gian tập', _formatDuration(_secondsElapsed)),
                  const SizedBox(height: 10),
                  _buildSummaryRow('Calo thực tế', '${_caloriesBurned.toStringAsFixed(1)} kcal'),
                  const SizedBox(height: 10),
                  _buildSummaryRow('Nhịp tim trung bình', '$_averageHeartRate bpm'),
                  const SizedBox(height: 10),
                  _buildSummaryRow('Nhịp tim tối đa', '$_maxHeartRate bpm'),
                ]),
              ),
            ],
          ),
          actions: [
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: () {
                  Navigator.pop(ctx); // Tắt success dialog
                  Navigator.pop(context, true); // Quay về Roadmap Screen
                },
                style: ElevatedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 14),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                ),
                child: const Text('Tuyệt vời, Đóng'),
              ),
            ),
          ],
        );
      },
    );
  }

  Widget _buildSummaryRow(String label, String value) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(label, style: GoogleFonts.inter(fontSize: 13, color: Colors.grey)),
        Text(value, style: GoogleFonts.outfit(fontSize: 14, fontWeight: FontWeight.bold)),
      ],
    );
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    
    // Nếu chưa bật Bluetooth, hiển thị màn hình yêu cầu
    if (!_isBluetoothOn) {
      return _buildBluetoothOffScreen(isDark);
    }

    // Nếu chưa kết nối thiết bị, hiển thị màn hình quét thiết bị BLE
    if (!_isConnecting && (_connectionState == BluetoothConnectionState.disconnected || _targetDevice == null)) {
      return _buildScanningScreen(isDark);
    }

    // Nếu đang kết nối (dùng cờ tự quản lý thay vì BluetoothConnectionState.connecting đã deprecated)
    if (_isConnecting) {
      return _buildConnectingScreen(isDark);
    }

    // Màn hình Workout Real-time khi đã kết nối
    return _buildLiveWorkoutScreen(isDark);
  }

  /// Giao diện Yêu cầu Bật Bluetooth
  Widget _buildBluetoothOffScreen(bool isDark) {
    return Scaffold(
      backgroundColor: isDark ? AppColors.darkBg : AppColors.lightBg,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => Navigator.pop(context),
        ),
      ),
      body: Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(mainAxisSize: MainAxisSize.min, children: [
            const Icon(Icons.bluetooth_disabled_rounded, size: 84, color: AppColors.error),
            const SizedBox(height: 24),
            Text(
              'Bluetooth Đang Tắt',
              style: GoogleFonts.outfit(fontSize: 22, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 12),
            Text(
              'Vui lòng bật kết nối Bluetooth trên điện thoại của bạn để có thể tìm và kết nối trực tiếp với đồng hồ Garmin Forerunner 55.',
              textAlign: TextAlign.center,
              style: GoogleFonts.inter(fontSize: 14, height: 1.6, color: Colors.grey),
            ),
          ]),
        ),
      ),
    );
  }

  /// Giao diện Quét thiết bị BLE Nhịp tim
  Widget _buildScanningScreen(bool isDark) {
    final cardBg = isDark ? AppColors.darkCard : AppColors.lightCard;
    return Scaffold(
      backgroundColor: isDark ? AppColors.darkBg : AppColors.lightBg,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        title: Text(
          'Kết Nối Đồng Hồ Garmin',
          style: GoogleFonts.outfit(fontSize: 18, fontWeight: FontWeight.bold),
        ),
        actions: [
          IconButton(
            icon: Icon(_isScanning ? Icons.sync_rounded : Icons.search_rounded),
            onPressed: _isScanning ? null : _startScan,
          ),
        ],
      ),
      body: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 20),
        child: Column(children: [
          const SizedBox(height: 16),
          // Radar pulse animation
          Center(
            child: TweenAnimationBuilder(
              tween: Tween(begin: 0.0, end: 1.0),
              duration: const Duration(seconds: 2),
              curve: Curves.easeOut,
              builder: (context, double val, child) {
                return Container(
                  width: 80, height: 80,
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    color: AppColors.auroraCyan.withValues(alpha: 0.1),
                    border: Border.all(
                      color: AppColors.auroraCyan.withValues(alpha: 1.0 - val),
                      width: 1 + val * 3,
                    ),
                  ),
                  child: const Icon(Icons.watch_rounded, color: AppColors.auroraCyan, size: 36),
                );
              },
              onEnd: () {},
            ),
          ),
          const SizedBox(height: 24),
          Text(
            _isScanning ? 'Đang tìm kiếm đồng hồ Garmin...' : 'Đã dừng tìm kiếm',
            style: GoogleFonts.inter(fontSize: 14, fontWeight: FontWeight.w500, color: Colors.grey),
          ),
          const SizedBox(height: 28),
          Expanded(
            child: _scanResults.isEmpty
                ? Center(
                    child: Text(
                      'Chưa tìm thấy thiết bị. Hãy bật chế độ "Broadcast Heart Rate" trên đồng hồ Garmin Forerunner 55 của bạn.',
                      textAlign: TextAlign.center,
                      style: GoogleFonts.inter(fontSize: 13, color: Colors.grey, height: 1.6),
                    ),
                  )
                : ListView.builder(
                    itemCount: _scanResults.length,
                    itemBuilder: (ctx, i) {
                      final result = _scanResults[i];
                      final name = result.device.platformName.isNotEmpty
                          ? result.device.platformName
                          : 'Cảm biến nhịp tim';
                      return Container(
                        margin: const EdgeInsets.only(bottom: 12),
                        padding: const EdgeInsets.all(16),
                        decoration: BoxDecoration(
                          color: cardBg,
                          borderRadius: BorderRadius.circular(20),
                          border: Border.all(
                            color: isDark ? Colors.white.withValues(alpha: 0.06) : Colors.black.withValues(alpha: 0.05),
                            width: 0.8,
                          ),
                        ),
                        child: Row(children: [
                          Container(
                            width: 44, height: 44,
                            decoration: BoxDecoration(
                              color: AppColors.auroraCyan.withValues(alpha: 0.12),
                              shape: BoxShape.circle,
                            ),
                            child: const Icon(Icons.watch_rounded, color: AppColors.auroraCyan, size: 20),
                          ),
                          const SizedBox(width: 14),
                          Expanded(
                            child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                              Text(
                                name,
                                style: GoogleFonts.outfit(fontSize: 15, fontWeight: FontWeight.bold),
                              ),
                              const SizedBox(height: 4),
                              Text(
                                'RSSI: ${result.rssi} dBm (Garmin Watch)',
                                style: GoogleFonts.inter(fontSize: 11, color: Colors.grey),
                              ),
                            ]),
                          ),
                          ElevatedButton(
                            onPressed: () => _connectToDevice(result.device),
                            style: ElevatedButton.styleFrom(
                              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                            ),
                            child: const Text('Kết nối'),
                          ),
                        ]),
                      );
                    },
                  ),
          ),
        ]),
      ),
    );
  }

  /// Giao diện Đang Kết Nối với thông tin trạng thái động chi tiết
  Widget _buildConnectingScreen(bool isDark) {
    return Scaffold(
      backgroundColor: isDark ? AppColors.darkBg : AppColors.lightBg,
      body: Center(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 24),
          child: Column(mainAxisSize: MainAxisSize.min, children: [
            const CircularProgressIndicator(color: AppColors.auroraCyan),
            const SizedBox(height: 24),
            Text(
              'Đang kết nối vào ${_targetDevice?.platformName ?? "Đồng hồ"}...',
              style: GoogleFonts.outfit(fontSize: 18, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 12),
            Text(
              _connectionStatusMessage,
              textAlign: TextAlign.center,
              style: GoogleFonts.inter(fontSize: 13, color: Colors.grey, height: 1.4),
            ),
          ]),
        ),
      ),
    );
  }

  /// Hiển thị Hộp thoại hướng dẫn chi tiết khi gặp lỗi kết nối BLE
  void _showConnectionErrorDialog(String error) {
    showDialog(
      context: context,
      builder: (ctx) {
        final isDark = Theme.of(context).brightness == Brightness.dark;
        return AlertDialog(
          backgroundColor: isDark ? AppColors.darkSurface : AppColors.lightSurface,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(24)),
          title: Row(
            children: [
              const Icon(Icons.warning_amber_rounded, color: AppColors.error, size: 28),
              const SizedBox(width: 10),
              Text(
                'Lỗi kết nối Garmin',
                style: GoogleFonts.outfit(fontSize: 18, fontWeight: FontWeight.bold),
              ),
            ],
          ),
          content: SingleChildScrollView(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Chi tiết lỗi: $error',
                  style: GoogleFonts.inter(fontSize: 12, color: AppColors.error, fontStyle: FontStyle.italic),
                ),
                const SizedBox(height: 16),
                Text(
                  'Vui lòng thực hiện các bước sau để khắc phục:',
                  style: GoogleFonts.inter(fontSize: 13, fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 12),
                _buildInstructionStep('1', 'Tắt ứng dụng Garmin Connect chạy ngầm', 'Hãy vuốt lên và đóng hoàn toàn ứng dụng Garmin Connect (nếu đang chạy ngầm) để giải phóng kênh truyền Bluetooth của đồng hồ.'),
                _buildInstructionStep('2', 'Kích hoạt chế độ "Phát nhịp tim"', 'Trên đồng hồ Forerunner 55: Giữ nút Menu (Up) -> Cảm biến & Phụ kiện (Sensors & Accessories) -> Nhịp tim cổ tay (Wrist Heart Rate) -> Phát nhịp tim (Broadcast Heart Rate) -> Bắt đầu (Start).'),
                _buildInstructionStep('3', 'Bật lại Bluetooth điện thoại', 'Tắt Bluetooth trên iPhone/Android khoảng 5 giây rồi bật lại để khởi động lại tiến trình BLE.'),
              ],
            ),
          ),
          actions: [
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: () => Navigator.pop(ctx),
                style: ElevatedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 12),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                ),
                child: const Text('Tôi đã hiểu, thử lại'),
              ),
            ),
          ],
        );
      },
    );
  }

  Widget _buildInstructionStep(String number, String title, String body) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            width: 22, height: 22,
            alignment: Alignment.center,
            decoration: const BoxDecoration(
              color: AppColors.auroraCyan,
              shape: BoxShape.circle,
            ),
            child: Text(
              number,
              style: const TextStyle(color: Colors.white, fontSize: 11, fontWeight: FontWeight.bold),
            ),
          ),
          const SizedBox(width: 10),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: GoogleFonts.inter(fontSize: 12, fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 2),
                Text(
                  body,
                  style: GoogleFonts.inter(fontSize: 11, color: Colors.grey, height: 1.4),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  /// Giao diện Luyện Tập Thời Gian Thực (Real-time Live Workout)
  Widget _buildLiveWorkoutScreen(bool isDark) {
    final textPrimary = isDark ? AppColors.darkText : AppColors.lightText;
    final cardBg = isDark ? AppColors.darkCard : AppColors.lightCard;
    
    // Lấy bài tập hiện tại
    final currentAct = _activities.isNotEmpty && _currentExerciseIndex < _activities.length
        ? _activities[_currentExerciseIndex]
        : null;
    final exerciseName = currentAct?['exercise']?['name'] ?? 'Hoàn thành bài tập';
    final sets = currentAct?['sets'] ?? currentAct?['setsCount'] ?? 0;
    final reps = currentAct?['repsPerSet'] ?? currentAct?['reps'] ?? 0;
    final phase = currentAct?['phase'] ?? 'Core Workout';

    return Scaffold(
      backgroundColor: isDark ? AppColors.darkBg : AppColors.lightBg,
      body: SafeArea(
        child: Column(children: [
          // ── Header Bar ────────────────────────────────────
          Padding(
            padding: const EdgeInsets.all(20),
            child: Row(children: [
              Expanded(
                child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text(
                    'Ngày Tập ${widget.dayIndex} — Live',
                    style: GoogleFonts.outfit(fontSize: 20, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 4),
                  Row(children: [
                    Container(
                      width: 8, height: 8,
                      decoration: const BoxDecoration(color: AppColors.success, shape: BoxShape.circle),
                    ),
                    const SizedBox(width: 6),
                    Text(
                      'Đồng hồ Garmin: Đang hoạt động',
                      style: GoogleFonts.inter(fontSize: 12, color: Colors.grey, fontWeight: FontWeight.w500),
                    ),
                  ]),
                ]),
              ),
              IconButton(
                icon: const Icon(Icons.power_settings_new_rounded, color: AppColors.error),
                onPressed: () => ref.read(bleHeartRateServiceProvider).disconnectDevice(),
              ),
            ]),
          ),

          // ── Vòng Tròn Nhịp Tim Đập Động (Live Pulse Ring) ───
          Expanded(
            flex: 4,
            child: Center(
              child: ScaleTransition(
                scale: Tween(begin: 0.96, end: 1.04).animate(
                  CurvedAnimation(parent: _pulseController, curve: Curves.easeInOut),
                ),
                child: Container(
                  width: 170, height: 170,
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    boxShadow: [
                      BoxShadow(
                        color: AppColors.error.withValues(alpha: 0.15),
                        blurRadius: 28,
                        spreadRadius: 8,
                      )
                    ],
                    gradient: const LinearGradient(
                      colors: [AppColors.error, AppColors.auroraPurple],
                      begin: Alignment.topLeft,
                      end: Alignment.bottomRight,
                    ),
                  ),
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      const Icon(Icons.favorite_rounded, color: Colors.white, size: 32),
                      const SizedBox(height: 6),
                      Text(
                        '$_currentHeartRate',
                        style: GoogleFonts.outfit(
                          fontSize: 48,
                          fontWeight: FontWeight.bold,
                          color: Colors.white,
                        ),
                      ),
                      Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Container(
                            width: 6,
                            height: 6,
                            decoration: BoxDecoration(
                              color: _packetCount > 0
                                  ? (_packetCount % 2 == 0 ? Colors.greenAccent : Colors.white)
                                  : Colors.white.withValues(alpha: 0.5),
                              shape: BoxShape.circle,
                            ),
                          ),
                          const SizedBox(width: 5),
                          Text(
                            _packetCount > 0 ? 'LIVE #$_packetCount' : 'BPM LIVE',
                            style: GoogleFonts.inter(
                              fontSize: 10,
                              fontWeight: FontWeight.bold,
                              color: Colors.white.withValues(alpha: 0.8),
                              letterSpacing: 1,
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),

          // ── Live Heart Rate Chart (Biểu Đồ Sóng Live) ──────
          Expanded(
            flex: 3,
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: _heartRateHistory.isEmpty
                  ? Container()
                  : LineChart(
                      LineChartData(
                        gridData: FlGridData(show: false),
                        titlesData: FlTitlesData(show: false),
                        borderData: FlBorderData(show: false),
                        lineBarsData: [
                          LineChartBarData(
                            spots: List.generate(_heartRateHistory.length, (i) {
                              return FlSpot(i.toDouble(), _heartRateHistory[i].toDouble());
                            }),
                            isCurved: true,
                            color: AppColors.error,
                            barWidth: 3.5,
                            isStrokeCapRound: true,
                            dotData: const FlDotData(show: false),
                            belowBarData: BarAreaData(
                              show: true,
                              gradient: LinearGradient(
                                colors: [
                                  AppColors.error.withValues(alpha: 0.3),
                                  AppColors.error.withValues(alpha: 0.0),
                                ],
                                begin: Alignment.topCenter,
                                end: Alignment.bottomCenter,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
            ),
          ),

          // ── Dashboard Các Chỉ Số (Calo, Thời Gian, Trực Tiếp) ──
          Expanded(
            flex: 2,
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: Row(children: [
                _buildMetricCard(
                  Icons.timer_rounded,
                  _formatDuration(_secondsElapsed),
                  'THỜI GIAN',
                  AppColors.auroraCyan,
                  cardBg,
                ),
                const SizedBox(width: 12),
                _buildMetricCard(
                  Icons.local_fire_department_rounded,
                  _caloriesBurned.toStringAsFixed(1),
                  'KCAL ĐÃ ĐỐT',
                  AppColors.darkCoral,
                  cardBg,
                ),
              ]),
            ),
          ),

          // ── Bài Tập Hiện Tại & Navigation Exercises ───────
          Container(
            margin: const EdgeInsets.all(20),
            padding: const EdgeInsets.all(18),
            decoration: BoxDecoration(
              color: cardBg,
              borderRadius: BorderRadius.circular(24),
              border: Border.all(
                color: isDark ? Colors.white.withValues(alpha: 0.06) : Colors.black.withValues(alpha: 0.05),
                width: 0.8,
              ),
            ),
            child: Column(children: [
              Row(children: [
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: AppColors.auroraCyan.withValues(alpha: 0.12),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Text(
                    phase,
                    style: GoogleFonts.inter(
                      fontSize: 10,
                      fontWeight: FontWeight.bold,
                      color: AppColors.auroraCyan,
                    ),
                  ),
                ),
                const Spacer(),
                if (currentAct?['exercise']?['instructionUrl'] != null &&
                    currentAct!['exercise']['instructionUrl'].toString().isNotEmpty)
                  IconButton(
                    icon: const Icon(Icons.play_circle_fill_rounded, color: AppColors.auroraCyan, size: 24),
                    padding: EdgeInsets.zero,
                    constraints: const BoxConstraints(),
                    onPressed: () {
                      _showLiveVideoDialog(context, currentAct!['exercise']);
                    },
                  ),
                const SizedBox(width: 8),
                Text(
                  'Bài ${_currentExerciseIndex + 1}/${_activities.length}',
                  style: GoogleFonts.inter(fontSize: 12, color: Colors.grey, fontWeight: FontWeight.bold),
                ),
              ]),
              const SizedBox(height: 12),
              Align(
                alignment: Alignment.centerLeft,
                child: Text(
                  exerciseName,
                  style: GoogleFonts.outfit(fontSize: 18, fontWeight: FontWeight.bold, color: textPrimary),
                ),
              ),
              const SizedBox(height: 4),
              Align(
                alignment: Alignment.centerLeft,
                child: Text(
                  sets > 0 ? '$sets hiệp × $reps lần' : 'Nhịp tự do theo hướng dẫn',
                  style: GoogleFonts.inter(fontSize: 13, color: Colors.grey),
                ),
              ),
              const SizedBox(height: 18),
              Row(children: [
                Expanded(
                  child: OutlinedButton(
                    onPressed: _currentExerciseIndex == 0
                        ? null
                        : () => setState(() => _currentExerciseIndex--),
                    style: OutlinedButton.styleFrom(
                      padding: const EdgeInsets.symmetric(vertical: 14),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                    ),
                    child: const Text('Bài trước'),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: ElevatedButton(
                    onPressed: _currentExerciseIndex >= _activities.length - 1
                        ? _finishWorkout
                        : () => setState(() => _currentExerciseIndex++),
                    style: ElevatedButton.styleFrom(
                      padding: const EdgeInsets.symmetric(vertical: 14),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                    ),
                    child: Text(_currentExerciseIndex >= _activities.length - 1
                        ? 'Hoàn thành'
                        : 'Bài tiếp theo'),
                  ),
                ),
              ]),
            ]),
          ),
        ]),
      ),
    );
  }

  Widget _buildMetricCard(IconData icon, String value, String label, Color accentColor, Color bg) {
    return Expanded(
      child: Container(
        padding: const EdgeInsets.symmetric(vertical: 14, horizontal: 16),
        decoration: BoxDecoration(
          color: bg,
          borderRadius: BorderRadius.circular(20),
        ),
        child: Row(children: [
          Icon(icon, color: accentColor, size: 28),
          const SizedBox(width: 12),
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Text(
                value,
                style: GoogleFonts.outfit(fontSize: 18, fontWeight: FontWeight.bold),
              ),
              Text(
                label,
                style: GoogleFonts.inter(fontSize: 9, fontWeight: FontWeight.bold, color: Colors.grey),
              ),
            ],
          ),
        ]),
      ),
    );
  }

  void _showLiveVideoDialog(BuildContext context, Map<String, dynamic> exercise) {
    final url = exercise['instructionUrl'] as String? ?? '';
    if (url.isEmpty) return;

    showDialog(
      context: context,
      builder: (ctx) {
        final isDark = Theme.of(context).brightness == Brightness.dark;
        return AlertDialog(
          backgroundColor: isDark ? AppColors.darkSurface : AppColors.lightSurface,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(24)),
          contentPadding: const EdgeInsets.all(16),
          title: Text(
            exercise['name'] ?? 'Hướng dẫn kỹ thuật',
            style: GoogleFonts.outfit(fontSize: 16, fontWeight: FontWeight.bold),
          ),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              WorkoutVideoPlayer(videoUrl: url, height: 180),
              const SizedBox(height: 14),
              Text(
                'Xem video để thực hiện đúng form động tác.',
                style: GoogleFonts.inter(fontSize: 12, color: Colors.grey),
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(ctx),
              child: const Text('Đóng'),
            ),
          ],
        );
      },
    );
  }
}
