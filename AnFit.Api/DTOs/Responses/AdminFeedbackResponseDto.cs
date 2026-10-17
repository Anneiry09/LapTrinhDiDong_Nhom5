using System;

namespace AnFit.Api.DTOs.Responses
{
    public class AdminFeedbackResponseDto
    {
        public int PlanId { get; set; }
        public string Type { get; set; } = string.Empty; // "MealPlan" hoặc "WorkoutPlan"
        public Guid UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string TargetName { get; set; } = string.Empty; // Danh sách Món ăn hoặc Bài tập nối chuỗi
        public int Rating { get; set; } // UserRating (1-5) hoặc DifficultyRating (1-5)
        public int Status { get; set; } // Trạng thái thực đơn / buổi tập
        public DateOnly PlanDate { get; set; }
        public DateTime FeedbackAt { get; set; }
    }
}
