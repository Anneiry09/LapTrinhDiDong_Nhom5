import 'dart:async';
import 'dart:math';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_blue_plus/flutter_blue_plus.dart';
import '../../core/theme/app_colors.dart';
import '../../providers/service_providers.dart';
import '../../widgets/anfit_toast.dart';

/// ═══════════════════════════════════════════════════════════
/// LiveWorkoutScreen — Tập luyện trực tiếp & Đo nhịp tim Garmin BLE
/// Tinh gọn, rõ ràng, tập trung vào tính năng hệ thống cốt lõi
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

class _LiveWorkoutScreenState extends ConsumerState<LiveWorkoutScreen> {
  // Trạng thái Bluetooth & Kết nối
  bool _isBluetoothOn = false;
  bool _isScanning = false;
  List<ScanResult> _scanResults = [];
  BluetoothDevice? _targetDevice;
  BluetoothConnectionState _connectionState = BluetoothConnectionState.disconnected;
  bool _isConnecting = false;
  String _statusMessage = 'Sẵn sàng kết nối';

  // Subscriptions & Timer
  StreamSubscription? _adapterStateSub;
  StreamSubscription? _scanResultsSub;
  StreamSubscription? _isScanningSub;
  StreamSubscription? _connStateSub;
  StreamSubscription? _hrSub;

  Timer? _workoutTimer;
  int _secondsElapsed = 0;
  int _currentHeartRate = 0;
  double _caloriesBurned = 0.0;
  final List<int> _heartRateHistory = [];

  // Thông số người dùng tính Calories (Công thức Keytel)
  double _userWeight = 70.0;
  int _userAge = 25;
  bool _userIsMale = true;

  // Danh sách bài tập và trạng thái hoàn thành
  late List<dynamic> _activities;
  final Set<int> _completedExercises = {};

  @override
  void initState() {
    super.initState();
    _activities = widget.plan['workoutActivities'] as List? ?? [];
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
    ref.read(bleHeartRateServiceProvider).disconnectDevice();
    super.dispose();
  }

  /// Tải thông tin người dùng từ server để tính toán calo tối ưu
  Future<void> _loadUserProfile() async {
    try {
      final profile = await ref.read(userServiceProvider).getProfile();
      if (mounted) {
        setState(() {
          _userWeight = (profile['weight'] ?? 70.0).toDouble();
          _userAge = profile['age'] ?? 25;
          _userIsMale = (profile['gender'] ?? 1) == 1;
        });
      }
    } catch (_) {}
  }

  /// Khởi tạo Bluetooth & lắng nghe adapter
  void _initBluetooth() {
    _adapterStateSub = FlutterBluePlus.adapterState.listen((state) {
      if (mounted) {
        setState(() => _isBluetoothOn = state == BluetoothAdapterState.on);
        if (_isBluetoothOn) _startScan();
      }
    });

    _scanResultsSub = FlutterBluePlus.scanResults.listen((results) {
      if (mounted) setState(() => _scanResults = results);
    });

    _isScanningSub = FlutterBluePlus.isScanning.listen((scanning) {
      if (mounted) setState(() => _isScanning = scanning);
    });
  }

  /// Quét thiết bị BLE đo nhịp tim
  Future<void> _startScan() async {
    setState(() => _scanResults = []);
    try {
      await ref.read(bleHeartRateServiceProvider).startScan();
    } catch (e) {
      if (mounted) AnFitToast.show(context, e.toString(), isError: true);
    }
  }

  /// Kết nối vào đồng hồ Garmin
  Future<void> _connectToDevice(BluetoothDevice device) async {
    setState(() {
      _targetDevice = device;
      _isConnecting = true;
      _statusMessage = 'Đang kết nối ${device.platformName}...';
    });

    try {
      final ble = ref.read(bleHeartRateServiceProvider);

      _connStateSub = ble.connectionStateStream.listen((state) {
        if (mounted) {
          setState(() => _connectionState = state);
          if (state == BluetoothConnectionState.disconnected) {
            _workoutTimer?.cancel();
          }
        }
      });

      _hrSub = ble.heartRateStream.listen((hr) {
        if (mounted) {
          setState(() {
            _currentHeartRate = hr;
            _heartRateHistory.add(hr);
            _updateCalories(hr);
          });
        }
      });

      await ble.connectToDevice(device, onProgress: (msg) {
        if (mounted) setState(() => _statusMessage = msg);
      });

      if (mounted) {
        setState(() {
          _isConnecting = false;
          _connectionState = BluetoothConnectionState.connected;
          _statusMessage = 'Đã kết nối';
        });
        _startWorkout();
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _isConnecting = false;
          _connectionState = BluetoothConnectionState.disconnected;
          _targetDevice = null;
        });
        AnFitToast.show(context, 'Không thể kết nối BLE: $e', isError: true);
      }
    }
  }

  /// Tính lượng Calo đốt cháy theo công thức Keytel (JSS, 2005)
  void _updateCalories(int hr) {
    if (hr <= 0) return;
    const double timeMinutes = 1.0 / 60.0;
    double cal = 0.0;
    if (_userIsMale) {
      cal = ((-55.0969 + (0.6309 * hr) + (0.1988 * _userWeight) + (0.2017 * _userAge)) / 4.184) * timeMinutes;
    } else {
      cal = ((-20.4022 + (0.4472 * hr) - (0.1263 * _userWeight) + (0.0740 * _userAge)) / 4.184) * timeMinutes;
    }
    _caloriesBurned += max(0.0, cal);
  }

  /// Bắt đầu đếm thời gian tập luyện
  void _startWorkout() {
    _workoutTimer?.cancel();
    _secondsElapsed = 0;
    _caloriesBurned = 0.0;
    _heartRateHistory.clear();
    _workoutTimer = Timer.periodic(const Duration(seconds: 1), (_) {
      if (mounted) setState(() => _secondsElapsed++);
    });
  }

  String _formatDuration(int seconds) {
    final m = (seconds ~/ 60).toString().padLeft(2, '0');
    final s = (seconds % 60).toString().padLeft(2, '0');
    return '$m:$s';
  }

  int get _averageHeartRate {
    if (_heartRateHistory.isEmpty) return 0;
    return (_heartRateHistory.reduce((a, b) => a + b) / _heartRateHistory.length).round();
  }

  /// Phân loại vùng nhịp tim
  (String, Color) _getHeartRateZone(int hr) {
    if (hr <= 0) return ('Chưa nhận tín hiệu', Colors.grey);
    if (hr < 100) return ('Khởi động', Colors.blue);
    if (hr < 130) return ('Đốt mỡ', Colors.green);
    if (hr < 160) return ('Cardio', Colors.orange);
    return ('Cực đại', Colors.red);
  }

  /// Hoàn thành buổi tập và gửi dữ liệu về C# Backend
  Future<void> _finishWorkout() async {
    _workoutTimer?.cancel();

    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (_) => const Center(child: CircularProgressIndicator()),
    );

    try {
      final planId = widget.plan['id'];
      // 1. Cập nhật trạng thái kế hoạch tập = 2 (Đã hoàn thành)
      await ref.read(workoutServiceProvider).updateStatus(planId, status: 2);

      // 2. Đồng bộ Calo và nhịp tim tối đa lên DailyHealthLogs
      final todayStr = DateTime.now().toIso8601String().substring(0, 10);
      final maxHr = _heartRateHistory.isNotEmpty ? _heartRateHistory.reduce(max) : _currentHeartRate;
      await ref.read(apiClientProvider).dio.post(
        '/DailyHealthLogs/sync-health-connect',
        data: {
          'totalSteps': 0,
          'activeCalories': double.parse(_caloriesBurned.toStringAsFixed(1)),
          'restingHeartRate': 60,
          'maxHeartRate': maxHr,
          'sleepDurationMinutes': 0,
          'stressLevel': 0.0,
          'targetDate': todayStr,
        },
      );

      if (mounted) {
        Navigator.pop(context); // Đóng loading
        _showSuccessDialog();
      }
    } catch (e) {
      if (mounted) {
        Navigator.pop(context);
        AnFitToast.show(context, 'Lỗi lưu kết quả: $e', isError: true);
      }
    }
  }

  void _showSuccessDialog() {
    final cs = Theme.of(context).colorScheme;
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (ctx) => AlertDialog(
        title: const Row(
          children: [
            Icon(Icons.check_circle, color: Colors.green, size: 28),
            SizedBox(width: 10),
            Text('Hoàn Thành Buổi Tập!'),
          ],
        ),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('⏱️ Thời gian: ${_formatDuration(_secondsElapsed)}'),
            const SizedBox(height: 6),
            Text('🔥 Calo tiêu hao: ${_caloriesBurned.toStringAsFixed(1)} kcal'),
            const SizedBox(height: 6),
            Text('💓 Nhịp tim trung bình: $_averageHeartRate bpm'),
          ],
        ),
        actions: [
          ElevatedButton(
            onPressed: () {
              Navigator.pop(ctx);
              Navigator.pop(context, true);
            },
            style: ElevatedButton.styleFrom(backgroundColor: cs.primary, foregroundColor: Colors.white),
            child: const Text('Xác nhận & Trở về'),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final isConnected = _connectionState == BluetoothConnectionState.connected;
    final (zoneName, zoneColor) = _getHeartRateZone(_currentHeartRate);

    return Scaffold(
      appBar: AppBar(
        title: Text('Buổi Tập Ngày ${widget.dayIndex}'),
        actions: [
          Container(
            margin: const EdgeInsets.only(right: 16),
            padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
            decoration: BoxDecoration(
              color: isConnected ? Colors.green.withValues(alpha: 0.15) : Colors.orange.withValues(alpha: 0.15),
              borderRadius: BorderRadius.circular(12),
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(Icons.bluetooth, size: 16, color: isConnected ? Colors.green : Colors.orange),
                const SizedBox(width: 4),
                Text(
                  isConnected
                      ? (_targetDevice?.platformName.isNotEmpty == true ? _targetDevice!.platformName : 'Đã kết nối')
                      : 'Chưa kết nối',
                  style: TextStyle(fontSize: 12, fontWeight: FontWeight.bold, color: isConnected ? Colors.green : Colors.orange),
                ),
              ],
            ),
          ),
        ],
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          // ── KHỐI KẾT NỐI BLE NẾU CHƯA KẾT NỐI ──
          if (!isConnected) ...[
            Card(
              elevation: 0,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(16),
                side: BorderSide(color: Theme.of(context).colorScheme.outline.withValues(alpha: 0.2)),
              ),
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        const Icon(Icons.watch, color: Colors.blue),
                        const SizedBox(width: 8),
                        const Expanded(
                          child: Text(
                            'Kết nối Đồng hồ Garmin (BLE)',
                            style: TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
                          ),
                        ),
                        if (_isScanning)
                          const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2))
                        else
                          TextButton.icon(
                            onPressed: _startScan,
                            icon: const Icon(Icons.refresh, size: 16),
                            label: const Text('Quét lại'),
                          ),
                      ],
                    ),
                    const SizedBox(height: 8),
                    Text(
                      _isBluetoothOn ? 'Hãy bật tính năng Broadcast Heart Rate trên Garmin.' : 'Vui lòng BẬT Bluetooth trên điện thoại.',
                      style: TextStyle(fontSize: 12, color: Colors.grey[600]),
                    ),
                    if (_isConnecting) ...[
                      const SizedBox(height: 8),
                      Row(
                        children: [
                          const SizedBox(width: 14, height: 14, child: CircularProgressIndicator(strokeWidth: 2)),
                          const SizedBox(width: 8),
                          Expanded(child: Text(_statusMessage, style: const TextStyle(fontSize: 12, color: Colors.blue))),
                        ],
                      ),
                    ],
                    if (_scanResults.isNotEmpty) ...[
                      const Divider(height: 20),
                      ..._scanResults.map((r) {
                        final name = r.device.platformName.isNotEmpty ? r.device.platformName : 'Thiết bị BLE (${r.device.remoteId.str.substring(0, 8)})';
                        return ListTile(
                          contentPadding: EdgeInsets.zero,
                          leading: const Icon(Icons.favorite_border, color: Colors.red),
                          title: Text(name, style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w600)),
                          subtitle: Text('RSSI: ${r.rssi} dBm', style: const TextStyle(fontSize: 11)),
                          trailing: ElevatedButton(
                            onPressed: _isConnecting ? null : () => _connectToDevice(r.device),
                            child: const Text('Kết nối'),
                          ),
                        );
                      }),
                    ],
                  ],
                ),
              ),
            ),
            const SizedBox(height: 16),
          ],

          // ── HERO CARD: CHỈ SỐ NHỊP TIM LỚN ──
          Container(
            padding: const EdgeInsets.all(24),
            decoration: BoxDecoration(
              color: isDark ? AppColors.darkCard : AppColors.lightCard,
              borderRadius: BorderRadius.circular(20),
              border: Border.all(color: zoneColor.withValues(alpha: 0.3), width: 1.5),
            ),
            child: Column(
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(Icons.favorite, color: zoneColor, size: 36),
                    const SizedBox(width: 12),
                    Text(
                      _currentHeartRate > 0 ? '$_currentHeartRate' : '--',
                      style: TextStyle(fontSize: 54, fontWeight: FontWeight.bold, color: zoneColor),
                    ),
                    const SizedBox(width: 6),
                    const Text('BPM', style: TextStyle(fontSize: 18, color: Colors.grey, fontWeight: FontWeight.bold)),
                  ],
                ),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                  decoration: BoxDecoration(
                    color: zoneColor.withValues(alpha: 0.15),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Text(
                    zoneName,
                    style: TextStyle(color: zoneColor, fontWeight: FontWeight.bold, fontSize: 13),
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),

          // ── STATS ROW: THỜI GIAN, CALO, NHỊP TIM TB ──
          Row(
            children: [
              _buildStatCard(context, 'Thời gian', _formatDuration(_secondsElapsed), Icons.timer_outlined, Colors.blue),
              const SizedBox(width: 10),
              _buildStatCard(context, 'Calo tiêu hao', '${_caloriesBurned.toStringAsFixed(1)} kcal', Icons.local_fire_department_outlined, Colors.orange),
              const SizedBox(width: 10),
              _buildStatCard(context, 'Nhịp tim TB', '$_averageHeartRate bpm', Icons.monitor_heart_outlined, Colors.teal),
            ],
          ),
          const SizedBox(height: 20),

          // ── DANH SÁCH BÀI TẬP TRONG BUỔI ──
          const Text(
            'Các bài tập hôm nay 📋',
            style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 8),
          ...List.generate(_activities.length, (i) {
            final a = _activities[i];
            final ex = a['exercise'] as Map<String, dynamic>? ?? {};
            final name = ex['name'] ?? 'Bài tập';
            final sets = a['sets'] ?? 3;
            final reps = a['repsPerSet'] ?? 12;
            final isDone = _completedExercises.contains(i);

            return Card(
              margin: const EdgeInsets.only(bottom: 8),
              elevation: 0,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
                side: BorderSide(color: isDone ? Colors.green.withValues(alpha: 0.5) : Colors.grey.withValues(alpha: 0.2)),
              ),
              child: CheckboxListTile(
                value: isDone,
                onChanged: (v) {
                  setState(() {
                    if (v == true) {
                      _completedExercises.add(i);
                    } else {
                      _completedExercises.remove(i);
                    }
                  });
                },
                activeColor: Colors.green,
                title: Text(
                  name,
                  style: TextStyle(
                    fontWeight: FontWeight.w600,
                    decoration: isDone ? TextDecoration.lineThrough : null,
                  ),
                ),
                subtitle: Text('$sets hiệp × $reps lần'),
              ),
            );
          }),

          const SizedBox(height: 24),

          // ── NÚT HOÀN THÀNH BUỔI TẬP ──
          SizedBox(
            width: double.infinity,
            child: ElevatedButton.icon(
              onPressed: _finishWorkout,
              icon: const Icon(Icons.check_circle_outline, color: Colors.white),
              label: const Text('Hoàn Thành Buổi Tập', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.white)),
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.green,
                padding: const EdgeInsets.symmetric(vertical: 16),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
              ),
            ),
          ),
          const SizedBox(height: 40),
        ],
      ),
    );
  }

  Widget _buildStatCard(BuildContext context, String label, String value, IconData icon, Color color) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Expanded(
      child: Container(
        padding: const EdgeInsets.all(12),
        decoration: BoxDecoration(
          color: isDark ? AppColors.darkCard : AppColors.lightCard,
          borderRadius: BorderRadius.circular(14),
          border: Border.all(color: Theme.of(context).colorScheme.outline.withValues(alpha: 0.1)),
        ),
        child: Column(
          children: [
            Icon(icon, color: color, size: 20),
            const SizedBox(height: 6),
            Text(value, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13)),
            const SizedBox(height: 2),
            Text(label, style: const TextStyle(fontSize: 10, color: Colors.grey)),
          ],
        ),
      ),
    );
  }
}
