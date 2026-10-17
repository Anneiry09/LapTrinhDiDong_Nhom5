using System;

namespace AnFit.Api.DTOs.Requests
{
    public class GenerateMealPlanRequest
    {
        public DateOnly PlanDate { get; set; }
        public double? TargetCalories { get; set; } // Nếu null, AI tự tính TDEE
        public decimal? BudgetLimit { get; set; }   // Ngân sách tối đa cho ngày đó
    }
}