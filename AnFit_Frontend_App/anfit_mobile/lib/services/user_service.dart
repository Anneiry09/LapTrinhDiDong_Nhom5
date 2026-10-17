import 'api_client.dart';

/// Service quản lý user profile, body logs, health logs
class UserService {
  final ApiClient _client;
  UserService(this._client);

  // ─── Profile ───

  /// Lấy thông tin user đang đăng nhập
  Future<Map<String, dynamic>> getProfile() async {
    final res = await _client.dio.get('/Users/me');
    return res.data['data'];
  }

  /// Cập nhật hồ sơ
  Future<Map<String, dynamic>> updateProfile(Map<String, dynamic> data) async {
    final res = await _client.dio.put('/Users/me', data: data);
    return res.data['data'];
  }

  /// Dashboard tổng hợp hôm nay
  Future<Map<String, dynamic>> getDashboard() async {
    final res = await _client.dio.get('/Dashboard/summary');
    return res.data['data'];
  }

  /// Xóa tài khoản (soft delete)
  Future<void> deleteAccount(String password) async {
    await _client.dio.delete('/Auth/delete-account', data: {
      'password': password,
      'confirmText': 'XOA TAI KHOAN',
    });
    await _client.clearTokens();
  }

  /// Đổi mật khẩu
  Future<void> changePassword(String currentPassword, String newPassword) async {
    await _client.dio.put('/Auth/change-password', data: {
      'currentPassword': currentPassword,
      'newPassword': newPassword,
    });
  }

  // ─── Body Logs ───

  /// Danh sách body log (cân nặng, BMI)
  Future<Map<String, dynamic>> getBodyLogs({int page = 1, int pageSize = 30}) async {
    final res = await _client.dio.get('/BodyLogs/history', queryParameters: {
      'page': page,
      'pageSize': pageSize,
    });
    return res.data;
  }

  /// Thêm log cân nặng mới
  Future<Map<String, dynamic>> addBodyLog(double weight, {double? bodyFatPercentage}) async {
    final res = await _client.dio.post('/BodyLogs', data: {
      'weight': weight,
      if (bodyFatPercentage != null) 'bodyFatPercentage': bodyFatPercentage,
    });
    return res.data['data'];
  }

  /// Lấy body log mới nhất
  Future<Map<String, dynamic>> getLatestBodyLog() async {
    final res = await _client.dio.get('/BodyLogs/latest');
    return res.data['data'];
  }

  // ─── Health Logs ───

  /// Danh sách health log (Garmin sync)
  Future<Map<String, dynamic>> getHealthLogs({String? from, String? to, int page = 1, int pageSize = 31}) async {
    final res = await _client.dio.get('/DailyHealthLogs', queryParameters: {
      if (from != null) 'from': from,
      if (to != null) 'to': to,
      'page': page,
      'pageSize': pageSize,
    });
    return res.data;
  }

  /// Health log hôm nay
  Future<Map<String, dynamic>?> getHealthLogToday() async {
    final res = await _client.dio.get('/DailyHealthLogs/today');
    return res.data['data'];
  }

  /// Biểu đồ health (steps, calories, hr, sleep)
  Future<Map<String, dynamic>> getHealthChart({required String from, required String to, required String metric}) async {
    final res = await _client.dio.get('/DailyHealthLogs', queryParameters: {
      'from': from,
      'to': to,
    });
    return res.data;
  }
}

