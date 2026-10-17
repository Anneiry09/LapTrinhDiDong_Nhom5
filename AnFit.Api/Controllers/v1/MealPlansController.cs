using AnFit.Api.Data;
using AnFit.Api.DTOs.Requests;
using AnFit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AnFit.Api.Controllers.v1
{
    [Authorize]
    [Route("api/v1/meal-plans")]
    [Route("api/v1/MealPlans")]
    public class MealPlansController : BaseApiController
    {
        private readonly IMealPlanService _mealPlanService;
        private readonly AnFit.Api.Repositories.Interfaces.IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public MealPlansController(IMealPlanService mealPlanService, AnFit.Api.Repositories.Interfaces.IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _mealPlanService = mealPlanService;
            _unitOfWork = unitOfWork;
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách recipes — lọc theo mealSuitability bitmask (1=B, 2=L, 4=D, 8=S)
        /// </summary>
        [HttpGet("recipes")]
        public async Task<IActionResult> GetRecipes([FromQuery] int? mealType)
        {
            var all = await _unitOfWork.Recipes.GetAllAsync();
            var recipes = all.AsEnumerable();

            if (mealType.HasValue)
            {
                int bit = mealType.Value switch
                {
                    1 => 1, // Breakfast
                    2 => 2, // Lunch
                    3 => 4, // Dinner
                    _ => 8  // Snack
                };
                recipes = recipes.Where(r => ((r.MealSuitability ?? 0) & bit) != 0);
            }

            var data = recipes.Select(r => new
            {
                r.Id,
                r.Name,
                totalCalories = r.TotalCalories ?? 0,
                totalProtein = r.TotalProtein ?? 0,
                totalCarbs = r.TotalCarbs ?? 0,
                totalFat = r.TotalFat ?? 0,
                estimatedPrice = r.EstimatedPrice ?? 0,
                r.Category
            }).ToList();

            return Ok(new { success = true, data });
        }

        /// <summary>
        /// Chi tiết 1 recipe — bao gồm danh sách nguyên liệu (Join FoodItems)
        /// </summary>
        [HttpGet("recipes/{id}")]
        public async Task<IActionResult> GetRecipeDetail(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Ingredients)
                    .ThenInclude(i => i.FoodItem)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
                return NotFound(new { success = false, message = "Không tìm thấy công thức." });

            var data = new
            {
                recipe.Id,
                recipe.Name,
                totalCalories = recipe.TotalCalories ?? 0,
                totalProtein = recipe.TotalProtein ?? 0,
                totalCarbs = recipe.TotalCarbs ?? 0,
                totalFat = recipe.TotalFat ?? 0,
                estimatedPrice = recipe.EstimatedPrice ?? 0,
                cookingTimeMinutes = recipe.CookingTimeMinutes ?? 0,
                recipe.Category,
                recipe.ImageUrl,
                ingredients = recipe.Ingredients.Select(i => new
                {
                    name = i.FoodItem?.Name ?? "Nguyên liệu",
                    quantityGrams = i.QuantityGrams ?? 0,
                    role = i.Role ?? 1
                }).ToList()
            };

            return Ok(new { success = true, data });
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetToday([FromQuery] DateOnly? date)
        {
            var userId = GetCurrentUserId();
            var queryDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
            var response = await _mealPlanService.GetByDateAsync(userId, queryDate);
            return Ok(new { success = true, data = response });
        }

        [HttpGet("roadmap")]
        public async Task<IActionResult> GetRoadmap([FromQuery] DateOnly? startDate)
        {
            var userId = GetCurrentUserId();
            var response = await _mealPlanService.GetMealRoadmapAsync(userId, startDate);
            return Ok(new { success = true, data = response });
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GeneratePlan([FromBody] GenerateMealPlanRequest request)
        {
            var userId = GetCurrentUserId();
            var response = await _mealPlanService.GeneratePlanAsync(userId, request);
            return Ok(new { success = true, message = "Đã tạo thực đơn bằng AI thành công.", data = response });
        }

        [HttpPost("generate-roadmap")]
        public async Task<IActionResult> GenerateRoadmap([FromQuery] bool force = false)
        {
            var userId = GetCurrentUserId();
            await _mealPlanService.Generate30DayMealRoadmapAsync(userId, force);
            return Ok(new { success = true, message = "Đã tạo lộ trình dinh dưỡng 30 ngày thành công." });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdatePlanStatusRequest request)
        {
            await _mealPlanService.UpdateStatusAsync(id, request);
            return Ok(new { success = true, message = "Đã cập nhật trạng thái thực đơn." });
        }

        [HttpPut("{id}/rate")]
        public async Task<IActionResult> RatePlan(int id, [FromBody] RatePlanRequest request)
        {
            await _mealPlanService.RatePlanAsync(id, request);
            return Ok(new { success = true, message = "Cảm ơn bạn đã đánh giá thực đơn." });
        }

        [HttpPut("details/{detailId}/swap")]
        public async Task<IActionResult> SwapMealItem(int detailId, [FromBody] SwapMealItemRequest request)
        {
            var response = await _mealPlanService.SwapMealItemAsync(detailId, request.NewRecipeId);
            return Ok(new { success = true, message = "Đã đổi món ăn thành công.", data = response });
        }

        [HttpPut("details/{detailId}/track")]
        public async Task<IActionResult> TrackMeal(int detailId)
        {
            await _mealPlanService.TrackMealAsync(detailId);
            return Ok(new { success = true, message = "Đã ghi nhận bạn đã ăn xong." });
        }
    }
}