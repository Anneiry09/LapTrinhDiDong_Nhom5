using AnFit.Api.Controllers;
using AnFit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AnFit.Api.Controllers.v1
{
    /// <summary>
    /// Controller chuyển tiếp yêu cầu AI đến Python Microservice.
    /// Kiến trúc: Flutter/Web → C# API Gateway → Python AI (Hybrid ML + Optimizer).
    /// </summary>
    [Authorize]
    public class AiController : BaseApiController
    {
        private readonly IAiService _aiService;

        public AiController(IAiService aiService)
        {
            _aiService = aiService;
        }

        /// <summary>
        /// 🤖 Hybrid AI: Sinh buổi tập linh hoạt theo tùy chỉnh của User.
        /// Kết hợp Machine Learning (Scorer) + Thuật toán Knapsack (Time Optimizer).
        /// </summary>
        /// <remarks>
        /// User chọn: Thời lượng tối đa (phút), Loại bài tập, Cấp độ, Khởi động/Giãn cơ.
        /// AI sẽ tự động chia phase (Warm-up → Core → Cool-down) và nhặt bài tập tối ưu nhất.
        /// 
        /// **Ví dụ Request:**
        /// ```json
        /// {
        ///   "user": { "age": 25, "bmi": 24.5, "isMale": 1, "targetGoal": 1, "fitnessLevel": 1 },
        ///   "maxDurationMins": 60,
        ///   "workoutType": 1,
        ///   "hasWarmup": true,
        ///   "hasCooldown": true,
        ///   "availableExercises": [
        ///     { "id": 1, "name": "Chạy bộ nhẹ", "baseCaloriesBurned": 25, "type": 2 },
        ///     { "id": 2, "name": "Hít đất", "baseCaloriesBurned": 50, "type": 1 },
        ///     { "id": 3, "name": "Plank", "baseCaloriesBurned": 25, "type": 1 },
        ///     { "id": 4, "name": "Yoga Giãn cơ", "baseCaloriesBurned": 15, "type": 5 }
        ///   ]
        /// }
        /// ```
        /// </remarks>
        [HttpPost("generate-custom-session")]
        public async Task<IActionResult> GenerateCustomSession([FromBody] AiCustomSessionRequest request)
        {
            var result = await _aiService.GenerateCustomSessionAsync(request);
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// 🤖 AI: Sinh thực đơn thông minh dựa trên TDEE, Budget và Dị ứng.
        /// Sử dụng thuật toán Greedy Algorithm để tối ưu Macro + Giá tiền.
        /// </summary>
        /// <remarks>
        /// C# Backend phải lọc sẵn các FoodItems an toàn (không vi phạm dị ứng) trước khi gửi sang Python.
        /// 
        /// **Ví dụ Request:**
        /// ```json
        /// {
        ///   "tdee": 2400,
        ///   "targetGoal": 1,
        ///   "dailyBudget": 80000,
        ///   "availableFoods": [
        ///     { "id": 1, "name": "Cơm trắng", "calories": 130, "protein": 2.7, "carbs": 28, "fat": 0.3, "price": 5000 },
        ///     { "id": 2, "name": "Ức gà luộc", "calories": 165, "protein": 31, "carbs": 0, "fat": 3.6, "price": 35000 }
        ///   ]
        /// }
        /// ```
        /// </remarks>
        [HttpPost("generate-meal-plan")]
        public async Task<IActionResult> GenerateMealPlan([FromBody] AiMealPlanRequest request)
        {
            var result = await _aiService.GenerateMealPlanAsync(request);
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// 🤖 AI: Sinh hướng dẫn nấu ăn on-demand (Gemini LLM + Fallback rule-based).
        /// </summary>
        [HttpPost("generate-instructions")]
        public async Task<IActionResult> GenerateInstructions([FromBody] AiInstructionsRequest request)
        {
            var result = await _aiService.GenerateInstructionsAsync(request);
            return Ok(new { success = true, data = result });
        }
    }
}
