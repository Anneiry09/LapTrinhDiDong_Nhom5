import 'dart:math';
import 'package:health/health.dart';
import 'api_client.dart';

/// Service quản lý đồng bộ dữ liệu từ Apple Health và Google Health Connect.
class HealthSyncService {
  final ApiClient _client;

  HealthSyncService(this._client);

  /// Danh sách các loại dữ liệu sức khỏe cần truy cập
  static final List<HealthDataType> _dataTypes = [
    HealthDataType.STEPS,
    HealthDataType.HEART_RATE,
    HealthDataType.ACTIVE_ENERGY_BURNED,
    HealthDataType.SLEEP_SESSION,
  ];

  /// Danh sách quyền tương ứng (chỉ đọc)
  static final List<HealthDataAccess> _permissions = [
    HealthDataAccess.READ,
    HealthDataAccess.READ,
    HealthDataAccess.READ,
    HealthDataAccess.READ,
  ];

  /// Yêu cầu cấp quyền truy cập Apple Health / Google Health Connect
  Future<bool> requestPermissions() async {
    try {
      // Cấu hình chế độ Health Connect trên Android
      await Health().configure();
      
      // Kiểm tra Health Connect đã được cài đặt chưa
      final status = await Health().getHealthConnectSdkStatus();
      print('Health Connect SDK status: $status');
      
      if (status != HealthConnectSdkStatus.sdkAvailable) {
        print('Health Connect SDK không khả dụng (status=$status). Vui lòng cài/cập nhật Health Connect.');
        return false;
      }
      
      bool? hasPermissions = await Health().hasPermissions(_dataTypes, permissions: _permissions);
      print('hasPermissions hiện tại: $hasPermissions');
      
      if (hasPermissions == null || !hasPermissions) {
        // Yêu cầu người dùng ủy quyền
        print('Đang yêu cầu người dùng cấp quyền Health Connect...');
        bool authorized = await Health().requestAuthorization(_dataTypes, permissions: _permissions);
        print('Kết quả requestAuthorization: $authorized');
        return authorized;
      }
      
      return true;
    } catch (e) {
      print('Lỗi khi yêu cầu quyền Health: $e');
      rethrow;
    }
  }

  /// Lấy dữ liệu sức khỏe cục bộ của ngày [targetDate] và đồng bộ lên Backend AnFit.
  /// Nếu Health Connect không khả dụng hoặc chưa cấp quyền, bỏ qua mà KHÔNG gửi dữ liệu giả.
  Future<Map<String, dynamic>> fetchAndSyncDailyData(DateTime targetDate) async {
    try {
      // 1. Kiểm tra và yêu cầu quyền
      bool hasPermission = false;
      try {
        hasPermission = await requestPermissions();
      } catch (e) {
        print('Không thể yêu cầu quyền Health Connect: $e');
        return _skipResult('Health Connect không khả dụng trên thiết bị này.');
      }

      if (!hasPermission) {
        print('Người dùng từ chối hoặc không cấp quyền Health Connect.');
        return _skipResult('Chưa cấp quyền truy cập Health Connect.');
      }

      // 2. Xác định khoảng thời gian của ngày cần đồng bộ (00:00:00 - 23:59:59)
      final startOfDay = DateTime(targetDate.year, targetDate.month, targetDate.day, 0, 0, 0);
      final endOfDay = DateTime(targetDate.year, targetDate.month, targetDate.day, 23, 59, 59);

      // 3. Lấy dữ liệu thô từ Health Kit/Health Connect
      List<HealthDataPoint> dataPoints = await Health().getHealthDataFromTypes(
        types: _dataTypes,
        startTime: startOfDay,
        endTime: endOfDay,
      );

      // 4. Phân tích và tính toán dữ liệu
      int totalSteps = 0;
      double activeCalories = 0.0;
      List<int> heartRates = [];
      int sleepMinutes = 0;

      int latestHeartRate = 0;
      DateTime? latestHeartRateTime;

      for (var point in dataPoints) {
        final val = point.value;

        switch (point.type) {
          case HealthDataType.STEPS:
            totalSteps += _parseNumericValue(val).toInt();
            break;
            
          case HealthDataType.ACTIVE_ENERGY_BURNED:
            activeCalories += _parseNumericValue(val).toDouble();
            break;
            
          case HealthDataType.HEART_RATE:
            int hr = _parseNumericValue(val).toInt();
            if (hr > 0) {
              heartRates.add(hr);
              // Lấy nhịp tim tức thời mới nhất dựa theo mốc thời gian ghi nhận
              if (latestHeartRateTime == null || point.dateFrom.isAfter(latestHeartRateTime)) {
                latestHeartRate = hr;
                latestHeartRateTime = point.dateFrom;
              }
            }
            break;
            
          case HealthDataType.SLEEP_SESSION:
            // Tính thời gian ngủ dựa trên khoảng thời gian của phiên ngủ
            final duration = point.dateTo.difference(point.dateFrom).inMinutes;
            sleepMinutes += duration;
            break;
            
          default:
            break;
        }
      }

      // Tính toán nhịp tim tối đa và nhịp tim nghỉ ngơi
      int maxHeartRate = 0;
      int restingHeartRate = 0;

      if (heartRates.isNotEmpty) {
        maxHeartRate = heartRates.reduce(max);
        
        // Tạo một bản sao để sắp xếp tìm Resting Heart Rate
        final sortedHeartRates = List<int>.from(heartRates);
        sortedHeartRates.sort();
        int sampleSize = max(1, (sortedHeartRates.length * 0.1).round());
        double avgLowHr = sortedHeartRates.take(sampleSize).reduce((a, b) => a + b) / sampleSize;
        restingHeartRate = avgLowHr.round();
      }

      // 5. Chuẩn bị Payload gửi lên API C#
      final payload = {
        'totalSteps': totalSteps,
        'activeCalories': activeCalories,
        'restingHeartRate': restingHeartRate,
        'maxHeartRate': maxHeartRate,
        'sleepDurationMinutes': sleepMinutes,
        'stressLevel': 0.0, // Health Connect không hỗ trợ Stress, mặc định là 0.0
        'targetDate': targetDate.toIso8601String().substring(0, 10), // định dạng YYYY-MM-DD
      };

      print('Đang đồng bộ dữ liệu Health Connect cục bộ lên Server AnFit...');
      print('Dữ liệu đồng bộ: $payload');

      // 6. Gửi API lên C# Backend
      final response = await _client.dio.post(
        '/DailyHealthLogs/sync-health-connect',
        data: payload,
      );

      if (response.statusCode == 200 || response.statusCode == 201) {
        return {
          'success': true,
          'message': 'Đồng bộ dữ liệu sức khỏe thành công!',
          'data': payload,
          'latestHeartRate': latestHeartRate,
        };
      }

      return {
        'success': false,
        'message': 'Đồng bộ thất bại. Server phản hồi mã lỗi: ${response.statusCode}',
        'latestHeartRate': latestHeartRate,
      };

    } catch (e) {
      print('Lỗi trong quá trình đồng bộ Health Connect: $e');
      return _skipResult('Không thể đồng bộ dữ liệu sức khỏe.');
    }
  }

  /// Trả về kết quả bỏ qua — KHÔNG gửi dữ liệu giả lên server
  Map<String, dynamic> _skipResult(String reason) {
    return {
      'success': false,
      'message': reason,
      'skipped': true,
    };
  }

  /// Đồng bộ dữ liệu lịch sử từ Health Connect cho [days] ngày gần nhất.
  /// Đọc dữ liệu từng ngày từ Health Connect local rồi gửi lên backend.
  /// Trả về số ngày đồng bộ thành công.
  Future<int> fetchAndSyncHistoricalData({int days = 30}) async {
    try {
      bool hasPermission = false;
      try {
        hasPermission = await requestPermissions();
      } catch (_) {
        return 0;
      }
      if (!hasPermission) return 0;

      int synced = 0;
      final now = DateTime.now();

      for (int i = 1; i <= days; i++) {
        final targetDate = now.subtract(Duration(days: i));
        final startOfDay = DateTime(targetDate.year, targetDate.month, targetDate.day, 0, 0, 0);
        final endOfDay = DateTime(targetDate.year, targetDate.month, targetDate.day, 23, 59, 59);

        try {
          List<HealthDataPoint> dataPoints = await Health().getHealthDataFromTypes(
            types: _dataTypes,
            startTime: startOfDay,
            endTime: endOfDay,
          );

          // Bỏ qua ngày không có dữ liệu
          if (dataPoints.isEmpty) continue;

          int totalSteps = 0;
          double activeCalories = 0.0;
          List<int> heartRates = [];
          int sleepMinutes = 0;

          for (var point in dataPoints) {
            final val = point.value;
            switch (point.type) {
              case HealthDataType.STEPS:
                totalSteps += _parseNumericValue(val).toInt();
                break;
              case HealthDataType.ACTIVE_ENERGY_BURNED:
                activeCalories += _parseNumericValue(val).toDouble();
                break;
              case HealthDataType.HEART_RATE:
                int hr = _parseNumericValue(val).toInt();
                if (hr > 0) heartRates.add(hr);
                break;
              case HealthDataType.SLEEP_SESSION:
                sleepMinutes += point.dateTo.difference(point.dateFrom).inMinutes;
                break;
              default:
                break;
            }
          }

          // Bỏ qua ngày không có chỉ số nào có giá trị
          if (totalSteps == 0 && activeCalories == 0 && heartRates.isEmpty && sleepMinutes == 0) continue;

          int maxHeartRate = 0;
          int restingHeartRate = 0;
          if (heartRates.isNotEmpty) {
            maxHeartRate = heartRates.reduce((a, b) => a > b ? a : b);
            final sorted = List<int>.from(heartRates)..sort();
            int sampleSize = max(1, (sorted.length * 0.1).round());
            restingHeartRate = (sorted.take(sampleSize).reduce((a, b) => a + b) / sampleSize).round();
          }

          final payload = {
            'totalSteps': totalSteps,
            'activeCalories': activeCalories,
            'restingHeartRate': restingHeartRate,
            'maxHeartRate': maxHeartRate,
            'sleepDurationMinutes': sleepMinutes,
            'stressLevel': 0.0,
            'targetDate': targetDate.toIso8601String().substring(0, 10),
          };

          await _client.dio.post('/DailyHealthLogs/sync-health-connect', data: payload);
          synced++;
        } catch (_) {
          // Bỏ qua lỗi từng ngày, tiếp tục sync ngày khác
          continue;
        }
      }

      return synced;
    } catch (_) {
      return 0;
    }
  }

  /// Hàm phụ trợ giúp parse dữ liệu dạng số an toàn, tránh lỗi Runtime Crash
  num _parseNumericValue(HealthValue value) {
    if (value is NumericHealthValue) {
      return value.numericValue;
    }
    
    // Fallback thử parse chuỗi nếu kiểu dữ liệu khác
    try {
      final str = value.toString();
      // Loại bỏ các chữ cái hoặc đơn vị nếu có (ví dụ: '80 bpm' -> '80')
      final numStr = RegExp(r'^[0-9.]+').stringMatch(str);
      if (numStr != null) {
        return num.tryParse(numStr) ?? 0;
      }
    } catch (_) {}
    
    return 0;
  }

  /// Lấy dữ liệu nhịp tim theo giờ từ Health Connect local (dùng cho biểu đồ 1 ngày)
  /// Trả về Map chứa: hourlyData (List), dailyMin, dailyMax, dailyAvg
  /// Nếu Health Connect không khả dụng, trả về dữ liệu rỗng (không sinh dữ liệu giả).
  Future<Map<String, dynamic>> fetchHourlyHeartRateData(DateTime targetDate) async {
    try {
      // Kiểm tra quyền
      bool hasPermission = false;
      try {
        hasPermission = await requestPermissions();
      } catch (e) {
        print('Không thể yêu cầu quyền: $e');
        return _emptyHourlyData();
      }

      if (!hasPermission) {
        return _emptyHourlyData();
      }

      final startOfDay = DateTime(targetDate.year, targetDate.month, targetDate.day, 0, 0, 0);
      final endOfDay = DateTime(targetDate.year, targetDate.month, targetDate.day, 23, 59, 59);

      // Chỉ lấy HEART_RATE
      List<HealthDataPoint> dataPoints = await Health().getHealthDataFromTypes(
        types: [HealthDataType.HEART_RATE],
        startTime: startOfDay,
        endTime: endOfDay,
      );

      if (dataPoints.isEmpty) {
        return _emptyHourlyData();
      }

      // Group theo giờ (0-23)
      Map<int, List<int>> hourlyMap = {};
      for (var point in dataPoints) {
        if (point.type == HealthDataType.HEART_RATE) {
          int hr = _parseNumericValue(point.value).toInt();
          if (hr > 0 && hr < 250) {
            int hour = point.dateFrom.hour;
            hourlyMap.putIfAbsent(hour, () => []);
            hourlyMap[hour]!.add(hr);
          }
        }
      }

      // Tính toán cho mỗi giờ
      List<Map<String, dynamic>> hourlyData = [];
      List<int> allHeartRates = [];

      for (int h = 0; h < 24; h++) {
        if (hourlyMap.containsKey(h) && hourlyMap[h]!.isNotEmpty) {
          final rates = hourlyMap[h]!;
          allHeartRates.addAll(rates);
          hourlyData.add({
            'hour': h,
            'avg': (rates.reduce((a, b) => a + b) / rates.length).round(),
            'min': rates.reduce(min),
            'max': rates.reduce(max),
            'count': rates.length,
          });
        }
      }

      int dailyMin = allHeartRates.isNotEmpty ? allHeartRates.reduce(min) : 0;
      int dailyMax = allHeartRates.isNotEmpty ? allHeartRates.reduce(max) : 0;
      int dailyAvg = allHeartRates.isNotEmpty
          ? (allHeartRates.reduce((a, b) => a + b) / allHeartRates.length).round()
          : 0;

      return {
        'hourlyData': hourlyData,
        'dailyMin': dailyMin,
        'dailyMax': dailyMax,
        'dailyAvg': dailyAvg,
        'hasData': hourlyData.isNotEmpty,
      };
    } catch (e) {
      print('Lỗi lấy dữ liệu nhịp tim theo giờ: $e');
      return _emptyHourlyData();
    }
  }

  /// Trả về dữ liệu rỗng cho biểu đồ nhịp tim theo giờ
  Map<String, dynamic> _emptyHourlyData() {
    return {
      'hourlyData': <Map<String, dynamic>>[],
      'dailyMin': 0,
      'dailyMax': 0,
      'dailyAvg': 0,
      'hasData': false,
    };
  }
}
