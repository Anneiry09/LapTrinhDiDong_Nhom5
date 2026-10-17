import 'api_client.dart';

/// Service quản lý meal plans + AI generate
class NutritionService {
  final ApiClient _client;
  NutritionService(this._client);

  /// Lấy meal plan theo ngày (mặc định hôm nay)
  Future<Map<String, dynamic>?> getTodayPlan({String? date}) async {
    final res = await _client.dio.get('/MealPlans/today', queryParameters: {
      // ignore: use_null_aware_elements
      if (date != null) 'date': date,
    });
    return res.data['data'];
  }

  /// AI tạo meal plan mới
  Future<Map<String, dynamic>> generatePlan({required String planDate, int? overrideBudget, List<int>? excludeFoodIds}) async {
    final res = await _client.dio.post('/MealPlans/generate', data: {
      'planDate': planDate,
      // ignore: use_null_aware_elements
      if (overrideBudget != null) 'overrideBudget': overrideBudget,
    });
    return res.data['data'];
  }

  /// Cập nhật trạng thái meal plan
  Future<void> updateStatus(int planId, {required int status}) async {
    await _client.dio.put('/MealPlans/$planId/status', data: {
      'status': status,
    });
  }

  /// Đánh giá meal plan
  Future<void> rate(int planId, {required int rating}) async {
    await _client.dio.put('/MealPlans/$planId/rate', data: {
      'rating': rating,
    });
  }

  /// Tìm kiếm thực phẩm
  Future<List<dynamic>> searchFoods(String query, {int page = 1}) async {
    final res = await _client.dio.get('/FoodItems', queryParameters: {
      'search': query,
      'page': page,
      'pageSize': 20,
    });
    return res.data['data'] as List;
  }

  /// Đổi món ăn trong bữa ăn (Swap) — chỉ cho phép recipe phù hợp cùng bữa
  Future<Map<String, dynamic>> swapMealItem(int detailId, int newRecipeId) async {
    final res = await _client.dio.put('/MealPlans/details/$detailId/swap', data: {
      'newRecipeId': newRecipeId,
    });
    return res.data['data'];
  }

  /// Xác nhận đã ăn xong (Track) — đánh dấu IsTracked = true
  Future<void> trackMeal(int detailId) async {
    await _client.dio.put('/MealPlans/details/$detailId/track');
  }

  /// Lấy danh sách recipes theo mealType (dùng cho màn hình Swap)
  Future<List<dynamic>> getRecipesByMealType(int mealType) async {
    final res = await _client.dio.get('/MealPlans/recipes', queryParameters: {
      'mealType': mealType,
    });
    return res.data['data'] as List;
  }

  /// Lấy danh sách thực phẩm theo category (backward compat)
  Future<List<dynamic>> getFoodsByCategory(int category) async {
    final res = await _client.dio.get('/FoodItems', queryParameters: {
      'category': category,
      'pageSize': 50,
    });
    return res.data['data'] as List;
  }

  /// Lấy lộ trình dinh dưỡng 30 ngày
  Future<List<dynamic>> getRoadmap({String? startDate}) async {
    final res = await _client.dio.get('/MealPlans/roadmap', queryParameters: {
      // ignore: use_null_aware_elements
      if (startDate != null) 'startDate': startDate,
    });
    return res.data['data'] as List;
  }

  /// AI tạo lộ trình 30 ngày
  Future<void> generateRoadmap({bool force = false}) async {
    await _client.dio.post(
      '/MealPlans/generate-roadmap',
      queryParameters: {
        'force': force,
      },
    );
  }

  /// Lấy chi tiết 1 recipe (nguyên liệu + macros)
  Future<Map<String, dynamic>> getRecipeDetail(int id) async {
    final res = await _client.dio.get('/MealPlans/recipes/$id');
    return res.data['data'];
  }

  /// AI sinh hướng dẫn nấu ăn (Gemini + fallback)
  Future<Map<String, dynamic>> generateInstructions({
    required String recipeName,
    required List<Map<String, dynamic>> ingredients,
  }) async {
    final res = await _client.dio.post('/Ai/generate-instructions', data: {
      'recipeName': recipeName,
      'ingredients': ingredients,
    });
    return res.data['data'];
  }
}
