using System;
using System.Collections.Generic;

namespace AnFit.Api.DTOs.Responses
{
    public class WorkoutPlanResponse
    {
        public int Id { get; set; }
        public DateOnly PlanDate { get; set; }
        public double TotalEstimatedCaloriesBurn { get; set; }
        public int DifficultyLevel { get; set; }
        public int Status { get; set; }
        public int? UserRating { get; set; }
        public IEnumerable<WorkoutActivityResponse> WorkoutActivities { get; set; } = new List<WorkoutActivityResponse>();
    }
}