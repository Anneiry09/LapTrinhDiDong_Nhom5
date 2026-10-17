import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../services/api_client.dart';
import '../services/auth_service.dart';
import '../services/workout_service.dart';
import '../services/nutrition_service.dart';
import '../services/user_service.dart';
import '../services/health_sync_service.dart';
import '../services/ble_heart_rate_service.dart';

/// Singleton ApiClient — dùng chung cho tất cả services
final apiClientProvider = Provider<ApiClient>((ref) => ApiClient());


/// Auth
final authServiceProvider = Provider<AuthService>((ref) {
  return AuthService(ref.watch(apiClientProvider));
});

/// Workout
final workoutServiceProvider = Provider<WorkoutService>((ref) {
  return WorkoutService(ref.watch(apiClientProvider));
});

/// Nutrition
final nutritionServiceProvider = Provider<NutritionService>((ref) {
  return NutritionService(ref.watch(apiClientProvider));
});

/// User (profile, body logs)
final userServiceProvider = Provider<UserService>((ref) {
  return UserService(ref.watch(apiClientProvider));
});

/// Health Sync / Local Health Connect & Apple Health
final healthSyncServiceProvider = Provider<HealthSyncService>((ref) {
  return HealthSyncService(ref.watch(apiClientProvider));
});

/// BLE Heart Rate Service
final bleHeartRateServiceProvider = Provider<BleHeartRateService>((ref) {
  return BleHeartRateService();
});

