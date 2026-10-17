using System;
using AnFit.Api.Models.Entities;

namespace AnFit.Api.DTOs.Responses
{
    public class DailyProgressResponse
    {
        public int Id { get; set; }
        
        public Guid UserId { get; set; }
        
        public DateOnly RecordDate { get; set; }
        
        // Dinh dưỡng
        public double TotalCaloriesConsumed { get; set; }
        public double TotalProtein { get; set; }
        public double TotalCarbs { get; set; }
        public double TotalFat { get; set; }
        
        // Tập luyện & Vận động
        public string? WorkoutPlanName { get; set; }
        public double TotalCaloriesBurned { get; set; }
        public int TotalSteps { get; set; }
        
        // Chỉ số cơ thể
        public double? CurrentWeight { get; set; }
        
        public DateTime CreatedAt { get; set; }

        public static DailyProgressResponse FromEntity(DailyProgressRecord entity)
        {
            return new DailyProgressResponse
            {
                Id = entity.Id,
                UserId = entity.UserId ?? Guid.Empty,
                RecordDate = entity.RecordDate ?? DateOnly.FromDateTime(DateTime.Today),
                TotalCaloriesConsumed = entity.TotalCaloriesConsumed ?? 0,
                TotalProtein = entity.TotalProtein ?? 0,
                TotalCarbs = entity.TotalCarbs ?? 0,
                TotalFat = entity.TotalFat ?? 0,
                WorkoutPlanName = entity.WorkoutPlanName,
                TotalCaloriesBurned = entity.TotalCaloriesBurned ?? 0,
                TotalSteps = entity.TotalSteps ?? 0,
                CurrentWeight = entity.CurrentWeight,
                CreatedAt = entity.CreatedAt ?? DateTime.UtcNow
            };
        }
    }
}
