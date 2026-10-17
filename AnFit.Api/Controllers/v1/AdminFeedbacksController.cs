using AnFit.Api.Data;
using AnFit.Api.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnFit.Api.Controllers.v1
{
    [Authorize(Roles = "Admin")]
    [Route("api/v1/admin/feedbacks")]
    public class AdminFeedbacksController : BaseApiController
    {
        private readonly ApplicationDbContext _context;

        public AdminFeedbacksController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách các kế hoạch ăn uống/luyện tập bị người dùng đánh giá thấp (1-3 sao)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLowRatedFeedbacks()
        {
            // 1. Lấy thực đơn (MealPlan) bị đánh giá thấp từ 1-3 sao
            var mealPlans = await _context.MealPlans
                .Include(mp => mp.User)
                .Include(mp => mp.MealPlanDetails)
                    .ThenInclude(d => d.Recipe)
                .Include(mp => mp.MealPlanDetails)
                    .ThenInclude(d => d.FoodItem)
                .Where(mp => mp.UserRating != null && mp.UserRating >= 1 && mp.UserRating <= 3)
                .ToListAsync();

            var mealFeedbacks = mealPlans.Select(mp => new AdminFeedbackResponseDto
            {
                PlanId = mp.Id,
                Type = "MealPlan",
                UserId = mp.UserId ?? Guid.Empty,
                UserFullName = mp.User?.FullName ?? "Người dùng ẩn danh",
                UserEmail = mp.User?.Email ?? "N/A",
                Rating = mp.UserRating ?? 0,
                Status = mp.Status ?? 0,
                PlanDate = mp.PlanDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                FeedbackAt = mp.UpdatedAt ?? mp.CreatedAt ?? DateTime.UtcNow,
                TargetName = string.Join(", ", mp.MealPlanDetails
                    .Select(d => d.Recipe != null ? d.Recipe.Name : (d.FoodItem != null ? d.FoodItem.Name : ""))
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Distinct())
            }).ToList();

            // 2. Lấy buổi tập (WorkoutPlan) bị đánh giá thấp từ 1-3 sao
            var workoutPlans = await _context.WorkoutPlans
                .Include(wp => wp.User)
                .Include(wp => wp.WorkoutActivities)
                    .ThenInclude(a => a.Exercise)
                .Where(wp => wp.DifficultyRating != null && wp.DifficultyRating >= 1 && wp.DifficultyRating <= 3)
                .ToListAsync();

            var workoutFeedbacks = workoutPlans.Select(wp => new AdminFeedbackResponseDto
            {
                PlanId = wp.Id,
                Type = "WorkoutPlan",
                UserId = wp.UserId ?? Guid.Empty,
                UserFullName = wp.User?.FullName ?? "Người dùng ẩn danh",
                UserEmail = wp.User?.Email ?? "N/A",
                Rating = wp.DifficultyRating ?? 0,
                Status = wp.Status ?? 0,
                PlanDate = wp.PlanDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                FeedbackAt = wp.UpdatedAt ?? wp.CreatedAt ?? DateTime.UtcNow,
                TargetName = string.Join(", ", wp.WorkoutActivities
                    .Select(a => a.Exercise != null ? a.Exercise.Name : "")
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Distinct())
            }).ToList();

            // 3. Kết hợp và sắp xếp giảm dần theo thời gian feedback (mới nhất lên đầu)
            var allFeedbacks = mealFeedbacks.Concat(workoutFeedbacks)
                .OrderByDescending(f => f.FeedbackAt)
                .ToList();

            return Ok(new { success = true, data = allFeedbacks });
        }
    }
}
