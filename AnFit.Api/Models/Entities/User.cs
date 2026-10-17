using System;
using System.Collections.Generic;

namespace AnFit.Api.Models.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int? Role { get; set; }

    public bool IsEmailVerified { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? ResetTokenExpiry { get; set; }

    public string? FullName { get; set; }

    public int? Gender { get; set; }

    public double? HeightCm { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public int? TargetGoal { get; set; }

    public decimal? DailyBudget { get; set; }

    // --- Onboarding Fields ---
    public int? Occupation { get; set; }

    public int? CurrentBodyShape { get; set; }

    public int? TargetBodyShape { get; set; }

    public int? FitnessLevel { get; set; }

    public int? PreferredIntensity { get; set; }

    public int? PsychologicalBarrier { get; set; }

    // --- Cài đặt dinh dưỡng ---
    public bool? IsCompensateExercise { get; set; }

    // --- Account Lockout ---
    public int AccessFailedCount { get; set; }

    public DateTime? LockoutEnd { get; set; }

    // --- Soft Delete ---
    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    // --- Audit ---
    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // --- Navigation Properties ---
    public virtual ICollection<DailyHealthLog> DailyHealthLogs { get; set; } = new List<DailyHealthLog>();

    public virtual ICollection<MealPlan> MealPlans { get; set; } = new List<MealPlan>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<UserBodyLog> UserBodyLogs { get; set; } = new List<UserBodyLog>();

    public virtual ICollection<UserIntegration> UserIntegrations { get; set; } = new List<UserIntegration>();

    public virtual ICollection<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();

    public virtual ICollection<MedicalCondition> Conditions { get; set; } = new List<MedicalCondition>();

    public virtual ICollection<ActivityCategory> PreferredActivities { get; set; } = new List<ActivityCategory>();

    public virtual ICollection<DailyProgressRecord> DailyProgressRecords { get; set; } = new List<DailyProgressRecord>();
}
