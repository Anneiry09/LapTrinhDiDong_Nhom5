using System;
using System.Collections.Generic;

namespace AnFit.Api.DTOs.Responses
{
    public class UserProfileResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int Role { get; set; }
        public int Gender { get; set; }
        public double HeightCm { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int TargetGoal { get; set; }
        public decimal DailyBudget { get; set; }
        public bool IsCompensateExercise { get; set; }
        public double? Weight { get; set; }
        public List<BodyLogResponse>? WeightHistory { get; set; }
    }
}