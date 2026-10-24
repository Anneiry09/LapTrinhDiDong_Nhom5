import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

/// Keys cho Secure Storage
class TokenKeys {
  static const accessToken = 'access_token';
  static const refreshToken = 'refresh_token';
}

/// Singleton API client — Dio + JWT interceptor + auto-refresh
class ApiClient {
  // Port khớp với launchSettings.json của backend (5108)
  // Windows desktop / Web: localhost:5108
  // Android emulator: 10.0.2.2:5108 (tương đương localhost của máy chủ)
  static String get defaultBaseUrl {
    // 1. Cho phép truyền URL tùy chỉnh khi build hoặc chạy qua --dart-define=API_URL=...
    const envUrl = String.fromEnvironment('API_URL');
    if (envUrl.isNotEmpty) {
      return envUrl;
    }

    if (kIsWeb) {
      return 'http://localhost:5108/api/v1';
    }
    if (defaultTargetPlatform == TargetPlatform.android) {
      return 'http://10.0.2.2:5108/api/v1';
    }
    return 'http://localhost:5108/api/v1';
  }

  final Dio dio;
  final FlutterSecureStorage _storage;

  ApiClient({FlutterSecureStorage? storage, String? baseUrl})
      : _storage = storage ?? const FlutterSecureStorage(),
        dio = Dio(BaseOptions(
          baseUrl: baseUrl ?? defaultBaseUrl,
          connectTimeout: const Duration(seconds: 10),
          receiveTimeout: const Duration(seconds: 30), // AI endpoints cần lâu hơn
          headers: {'Content-Type': 'application/json'},
        )) {
    dio.interceptors.add(InterceptorsWrapper(
      onRequest: _onRequest,
      onError: _onError,
    ));
  }

  /// Gắn Bearer token vào mỗi request
  Future<void> _onRequest(RequestOptions options, RequestInterceptorHandler handler) async {
    final token = await _storage.read(key: TokenKeys.accessToken);
    if (token != null) {
      options.headers['Authorization'] = 'Bearer $token';
    }
    handler.next(options);
  }

  /// Auto-refresh khi nhận 401
  Future<void> _onError(DioException err, ErrorInterceptorHandler handler) async {
    if (err.response?.statusCode == 401) {
      try {
        final refreshed = await _refreshToken();
        if (refreshed) {
          // Retry request gốc với token mới
          final opts = err.requestOptions;
          final token = await _storage.read(key: TokenKeys.accessToken);
          opts.headers['Authorization'] = 'Bearer $token';
          final response = await dio.fetch(opts);
          return handler.resolve(response);
        }
      } catch (_) {
        // Refresh thất bại → clear tokens
        await clearTokens();
      }
    }
    handler.next(err);
  }

  Future<bool> _refreshToken() async {
    final refresh = await _storage.read(key: TokenKeys.refreshToken);
    if (refresh == null) return false;

    try {
      final res = await Dio(BaseOptions(baseUrl: dio.options.baseUrl))
          .post('/Auth/refresh-token', data: {'refreshToken': refresh});
      if (res.statusCode == 200 && res.data['success'] == true) {
        await saveTokens(res.data['data']);
        return true;
      }
    } catch (_) {}
    return false;
  }

  /// Lưu token pair vào secure storage
  Future<void> saveTokens(Map<String, dynamic> data) async {
    await _storage.write(key: TokenKeys.accessToken, value: data['accessToken']);
    await _storage.write(key: TokenKeys.refreshToken, value: data['refreshToken']);
  }

  /// Xóa tokens (dùng khi logout)
  Future<void> clearTokens() async {
    await _storage.delete(key: TokenKeys.accessToken);
    await _storage.delete(key: TokenKeys.refreshToken);
  }

  /// Kiểm tra có token hay không (dùng cho auto-login check)
  Future<bool> hasToken() async {
    final token = await _storage.read(key: TokenKeys.accessToken);
    return token != null && token.isNotEmpty;
  }

  /// Lấy error message thân thiện từ DioException
  static String errorMessage(DioException e) {
    if (e.response?.data is Map) {
      final data = e.response!.data as Map;
      if (data['error'] is Map) return data['error']['message'] ?? 'Lỗi không xác định';
      if (data['message'] is String) return data['message'];
    }
    switch (e.type) {
      case DioExceptionType.connectionTimeout:
      case DioExceptionType.receiveTimeout:
        return 'Kết nối quá lâu, vui lòng thử lại.';
      case DioExceptionType.connectionError:
        return 'Không thể kết nối server. Kiểm tra mạng!';
      default:
        return 'Đã xảy ra lỗi (${e.response?.statusCode ?? 'unknown'}).';
    }
  }
}
