import 'package:dio/dio.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class AuthInterceptor extends Interceptor {
  final _storage = const FlutterSecureStorage();
  
  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) async {
    // Không cần token cho login và register
    if (!options.path.contains('/auth/login') && !options.path.contains('/auth/register')) {
      final token = await _storage.read(key: 'accessToken');
      if (token != null) {
        options.headers['Authorization'] = 'Bearer $token';
      }
    }
    super.onRequest(options, handler);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
    // Xử lý 401 Unauthorized (Token hết hạn)
    if (err.response?.statusCode == 401) {
      // TODO: Logic refresh token hoặc redirect người dùng về trang đăng nhập
      // await _storage.delete(key: 'accessToken');
    }
    super.onError(err, handler);
  }
}
