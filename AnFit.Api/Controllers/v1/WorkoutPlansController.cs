using AnFit.Api.DTOs.Requests;
using AnFit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AnFit.Api.Controllers.v1
{
    [Authorize]
    [Route("api/v1/workout-plans")]
    [Route("api/v1/WorkoutPlans")]
    public class WorkoutPlansController : BaseApiController
    {
        private readonly IWorkoutPlanService _workoutPlanService;

        public WorkoutPlansController(IWorkoutPlanService workoutPlanService)
        {
            _workoutPlanService = workoutPlanService;
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetToday([FromQuery] DateOnly? date)
        {
            var userId = GetCurrentUserId();
            var queryDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
            var response = await _workoutPlanService.GetByDateAsync(userId, queryDate);
            return Ok(new { success = true, data = response });
        }

        [HttpGet("roadmap")]
        public async Task<IActionResult> GetRoadmap([FromQuery] DateOnly? startDate)
        {
            var userId = GetCurrentUserId();
            var response = await _workoutPlanService.GetRoadmapAsync(userId, startDate);
            return Ok(new { success = true, data = response });
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GeneratePlan([FromBody] GenerateWorkoutPlanRequest request)
        {
            var userId = GetCurrentUserId();
            var response = await _workoutPlanService.GeneratePlanAsync(userId, request);
            return Ok(new { success = true, message = "Đã tạo lịch tập bằng AI thành công.", data = response });
        }

        [HttpPost("generate-roadmap")]
        public async Task<IActionResult> GenerateRoadmap([FromQuery] bool force = false)
        {
            var userId = GetCurrentUserId();
            await _workoutPlanService.Generate30DayRoadmapAsync(userId, force);
            return Ok(new { success = true, message = "Đã tạo lộ trình tập luyện 30 ngày thành công." });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdatePlanStatusRequest request)
        {
            await _workoutPlanService.UpdateStatusAsync(id, request);
            return Ok(new { success = true, message = "Đã cập nhật trạng thái bài tập." });
        }

        [HttpPut("{id}/rate")]
        public async Task<IActionResult> RatePlan(int id, [FromBody] RatePlanRequest request)
        {
            await _workoutPlanService.RatePlanAsync(id, request);
            return Ok(new { success = true, message = "Cảm ơn bạn đã đánh giá lịch tập." });
        }
    }
}