using System;

namespace AnFit.Api.DTOs.Requests
{
    public class GenerateWorkoutPlanRequest
    {
        public DateOnly PlanDate { get; set; }
        public int DifficultyLevel { get; set; } = 2; // 1: Dễ, 2: Vừa, 3: Khó
        public int? MaxDurationMinutes { get; set; } = 60; // Thời lượng tối đa (phút)
        public int? WorkoutType { get; set; } = 1; // 1: Bodyweight, 2: Cardio, 3: Dumbbell, 4: Machine, 5: Yoga
        public System.Collections.Generic.List<string>? TargetMuscleGroups { get; set; }
        public bool HasWarmup { get; set; } = true;
        public bool HasCooldown { get; set; } = true;
    }
}