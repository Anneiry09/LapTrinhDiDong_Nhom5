using System;

namespace AnFit.Api.DTOs.Responses
{
    public class DashboardSummaryResponse
    {
        public double CurrentWeight { get; set; }
        public double TodayTdee { get; set; }
        public MealPlanResponse? TodayMealPlan { get; set; }
        public WorkoutPlanResponse? TodayWorkoutPlan { get; set; }
        public DailyHealthLogResponse? TodayHealthStatus { get; set; }
    }
}