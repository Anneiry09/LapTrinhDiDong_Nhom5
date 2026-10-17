import 'api_client.dart';

/// Service xác thực — login, register, logout
class AuthService {
  final ApiClient _client;
  AuthService(this._client);

  /// Đăng nhập
  Future<Map<String, dynamic>> login(String email, String password) async {
    final res = await _client.dio.post('/Auth/login', data: {
      'email': email,
      'password': password,
    });
    final data = res.data['data'] as Map<String, dynamic>;
    await _client.saveTokens(data);
    return data;
  }

  /// Đăng ký kèm Onboarding data (Frictionless flow)
  Future<Map<String, dynamic>> registerWithOnboarding(Map<String, dynamic> body) async {
    final res = await _client.dio.post('/Auth/register-with-onboarding', data: body);
    final data = res.data['data'] as Map<String, dynamic>;
    await _client.saveTokens(data);
    return data;
  }

  /// Gửi OTP đăng ký
  Future<Map<String, dynamic>> sendOtpRegister(String email) async {
    final res = await _client.dio.post('/Auth/send-otp-register', data: {
      'email': email,
    });
    return res.data as Map<String, dynamic>;
  }

  /// Đăng xuất — hủy refresh token phía server
  Future<void> logout() async {
    try {
      await _client.dio.post('/Auth/revoke-token');
    } catch (_) {
      // Bỏ qua lỗi server, vẫn clear local
    }
    await _client.clearTokens();
  }

  /// Yêu cầu gửi mã khôi phục mật khẩu (OTP)
  Future<Map<String, dynamic>> forgotPassword(String email) async {
    final res = await _client.dio.post('/Auth/forgot-password', data: {
      'email': email,
    });
    return res.data as Map<String, dynamic>;
  }

  /// Đặt lại mật khẩu mới bằng OTP
  Future<Map<String, dynamic>> resetPassword({
    required String token,
    required String newPassword,
    required String confirmPassword,
  }) async {
    final res = await _client.dio.post('/Auth/reset-password', data: {
      'token': token,
      'newPassword': newPassword,
      'confirmPassword': confirmPassword,
    });
    return res.data as Map<String, dynamic>;
  }

  /// Kiểm tra đã đăng nhập chưa
  Future<bool> isLoggedIn() => _client.hasToken();
}

