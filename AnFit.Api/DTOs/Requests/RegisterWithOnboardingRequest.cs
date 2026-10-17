using System;
using System.Collections.Generic;

namespace AnFit.Api.DTOs.Requests
{
    public class RegisterWithOnboardingRequest
    {
        // --- Auth ---
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;

        // --- Bio (GĐ 3) ---
        public int? Gender { get; set; }
        public double? HeightCm { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public double? Weight { get; set; } // Cân nặng ban đầu → ghi vào UserBodyLogs

        // --- Goal & Budget (GĐ 2) ---
        public int? TargetGoal { get; set; }
        public decimal? DailyBudget { get; set; }

        // --- Onboarding (GĐ 2, 3, 6, 8) ---
        public int? Occupation { get; set; }
        public int? CurrentBodyShape { get; set; }
        public int? TargetBodyShape { get; set; }
        public int? FitnessLevel { get; set; }
        public int? PreferredIntensity { get; set; }
        public int? PsychologicalBarrier { get; set; }

        // --- Many-to-Many (GĐ 4, 6) ---
        public List<int> MedicalConditionIds { get; set; } = new();
        public List<int> PreferredActivityIds { get; set; } = new();
    }
}
