import 'api_client.dart';

/// Service quản lý workout plans + AI generate
class WorkoutService {
  final ApiClient _client;
  WorkoutService(this._client);

  /// Lấy workout plan hôm nay
  Future<Map<String, dynamic>?> getTodayPlan() async {
    final res = await _client.dio.get('/WorkoutPlans/today');
    return res.data['data'];
  }

  /// Lấy lộ trình 30 ngày tập luyện
  Future<List<Map<String, dynamic>>> getRoadmap(String startDate) async {
    final res = await _client.dio.get('/WorkoutPlans/roadmap', queryParameters: {
      'startDate': startDate,
    });
    return List<Map<String, dynamic>>.from(res.data['data'] ?? []);
  }

  /// AI tạo workout plan mới
  Future<Map<String, dynamic>> generatePlan({
    required String planDate, 
    int? maxMinutes, 
    int? difficulty, 
    int? workoutType,
    List<String>? targetMuscleGroups,
    bool hasWarmup = true,
    bool hasCooldown = true,
  }) async {
    final res = await _client.dio.post('/WorkoutPlans/generate', data: {
      'planDate': planDate,
      'difficultyLevel': difficulty ?? 2,
      'maxDurationMinutes': maxMinutes ?? 60,
      'workoutType': workoutType ?? 1,
      'hasWarmup': hasWarmup,
      'hasCooldown': hasCooldown,
      if (targetMuscleGroups != null) 'targetMuscleGroups': targetMuscleGroups,
    });
    return res.data['data'];
  }

  /// AI tạo buổi tập custom (On-Demand via FastAPI)
  Future<Map<String, dynamic>> generateCustomSession(Map<String, dynamic> body) async {
    final res = await _client.dio.post('/Ai/generate-custom-session', data: body);
    return res.data['data'];
  }

  /// Cập nhật trạng thái buổi tập
  Future<void> updateStatus(int planId, {required int status}) async {
    await _client.dio.put('/WorkoutPlans/$planId/status', data: {
      'status': status,
    });
  }

  /// Đánh giá buổi tập
  Future<void> rate(int planId, {required int rating}) async {
    await _client.dio.put('/WorkoutPlans/$planId/rate', data: {
      'rating': rating,
    });
  }

  /// AI tạo lộ trình 30 ngày tập luyện
  Future<void> generateRoadmap({bool force = false}) async {
    await _client.dio.post(
      '/WorkoutPlans/generate-roadmap',
      queryParameters: {
        'force': force,
      },
    );
  }
}
