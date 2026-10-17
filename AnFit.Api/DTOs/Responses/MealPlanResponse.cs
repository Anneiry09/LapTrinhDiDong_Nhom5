using System;
using System.Collections.Generic;

namespace AnFit.Api.DTOs.Responses
{
    public class MealPlanResponse
    {
        public int Id { get; set; }
        public DateOnly PlanDate { get; set; }
        public double TotalCalories { get; set; }
        public double TotalProtein { get; set; }
        public double TotalCarbs { get; set; }
        public double TotalFat { get; set; }
        public decimal TotalPrice { get; set; }
        public int Status { get; set; }
        public int? UserRating { get; set; }

        // Budget fields
        public decimal DailyBudget { get; set; }
        public double BudgetUsedPercent { get; set; }
        public List<string> BudgetWarnings { get; set; } = new();

        public IEnumerable<MealPlanDetailResponse> MealPlanDetails { get; set; } = new List<MealPlanDetailResponse>();
    }
}