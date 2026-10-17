import 'package:dio/dio.dart';
import 'auth_interceptor.dart';

class ApiClient {
  static const String baseUrl = 'https://10.0.2.2:7240/api/v1'; // Default for Android Emulator
  
  static final Dio _dio = Dio(
    BaseOptions(
      baseUrl: baseUrl,
      connectTimeout: const Duration(seconds: 15),
      receiveTimeout: const Duration(seconds: 15),
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
    ),
  );

  static void initialize() {
    _dio.interceptors.add(AuthInterceptor());
    // Thêm Logging Interceptor nếu cần debug
    _dio.interceptors.add(LogInterceptor(responseBody: true, requestBody: true));
  }

  static Dio get instance => _dio;
}
