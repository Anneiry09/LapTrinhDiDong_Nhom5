using System;

namespace AnFit.Api.DTOs.Requests
{
    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public int Gender { get; set; }
        public double HeightCm { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int TargetGoal { get; set; }
        public decimal DailyBudget { get; set; }
        public bool IsCompensateExercise { get; set; } = true;
    }
}