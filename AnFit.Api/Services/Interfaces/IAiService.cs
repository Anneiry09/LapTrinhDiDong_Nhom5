using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Interfaces
{
    /// <summary>
    /// Interface giao tiếp với AI Microservice (Python FastAPI).
    /// Hỗ trợ 2 chế độ: Sinh buổi tập linh hoạt (On-Demand) và Sinh thực đơn thông minh.
    /// Kiến trúc Hybrid AI: Machine Learning (Scorer) + Thuật toán tối ưu (Knapsack).
    /// </summary>
    public interface IAiService
    {
        /// <summary>
        /// Chế độ 2: Gọi AI để sinh 1 buổi tập linh hoạt theo tùy chỉnh của User.
        /// User chọn: Thời lượng, Thể loại, Cấp độ, Khởi động/Giãn cơ.
        /// AI sẽ dùng ML Scorer + Knapsack để nhặt bài tập tối ưu nhất.
        /// </summary>
        Task<AiCustomSessionResult> GenerateCustomSessionAsync(AiCustomSessionRequest request);

        /// <summary>
        /// Gọi AI để sinh thực đơn thông minh dựa trên TDEE, Budget và Dị ứng.
        /// </summary>
        Task<AiMealPlanResult> GenerateMealPlanAsync(AiMealPlanRequest request);

        /// <summary>
        /// Gọi AI để sinh hướng dẫn nấu ăn (Gemini LLM + Fallback rule-based).
        /// </summary>
        Task<AiInstructionsResult> GenerateInstructionsAsync(AiInstructionsRequest request);
    }

    // ===================================================
    // DTOs cho Custom Workout Session (Hybrid AI)
    // ===================================================

    public class AiUserContext
    {
        public double Age { get; set; }
        public double Bmi { get; set; }
        public int IsMale { get; set; }       // 1: Nam, 0: Nữ
        public int TargetGoal { get; set; }    // 1: Lose, 2: Maintain, 3: Gain
        public int FitnessLevel { get; set; }  // 1: Beginner, 2: Intermediate, 3: Advanced
    }

    public class AiAvailableExercise
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double BaseCaloriesBurned { get; set; }
        public int Type { get; set; } // 1: Bodyweight, 2: Cardio, 3: Dumbbell, 4: Machine, 5: Yoga
        public string MuscleGroup { get; set; } = string.Empty;
    }

    public class AiCustomSessionRequest
    {
        public AiUserContext User { get; set; } = new();
        public int MaxDurationMins { get; set; }
        public int WorkoutType { get; set; }
        public bool HasWarmup { get; set; }
        public bool HasCooldown { get; set; }
        public List<string>? TargetMuscleGroups { get; set; }
        public List<AiAvailableExercise> AvailableExercises { get; set; } = new();
    }

    public class AiCustomSessionResult
    {
        public string Status { get; set; } = string.Empty;
        public double TotalDurationMins { get; set; }
        public double TotalEstimatedCalories { get; set; }
        public List<AiWorkoutPhase> WorkoutPlan { get; set; } = new();
    }

    public class AiWorkoutPhase
    {
        public string Phase { get; set; } = string.Empty;
        public int ExerciseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double DurationMins { get; set; }
        public double CaloriesBurned { get; set; }
        public double? MlSuitabilityScore { get; set; }
        public int? Sets { get; set; }
        public int? Reps { get; set; }
        public int? RestTimeSeconds { get; set; }
    }

    // ===================================================
    // DTOs cho Meal Plan Generator (Recipe-based)
    // ===================================================

    // Giữ lại cho backward compat (SwapMealItem cũ)
    public class AiAvailableFood
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }
        public double Price { get; set; }
        public int Category { get; set; }
    }

    public class AiAvailableRecipe
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }
        public double Price { get; set; }
        public int MealSuitability { get; set; }  // bitmask: 1=B, 2=L, 4=D, 8=S
        public int Category { get; set; }
    }

    public class AiMealPlanRequest
    {
        public double Tdee { get; set; }
        public int TargetGoal { get; set; }
        public double DailyBudget { get; set; }
        public List<AiAvailableRecipe> AvailableRecipes { get; set; } = new();
        /// <summary>
        /// IDs của recipes đã dùng 2-3 ngày gần đây (Repetition Penalty).
        /// Python sẽ phạt điểm các recipes này để tránh lặp món.
        /// </summary>
        public List<int> RecentRecipeIds { get; set; } = new();
    }

    public class AiMealPlanResult
    {
        public string Status { get; set; } = string.Empty;
        public double TargetCalories { get; set; }
        public double GeneratedCalories { get; set; }
        public double TotalPrice { get; set; }
        public double BudgetLimit { get; set; }
        public double BudgetUsedPercent { get; set; }
        public List<string> Warnings { get; set; } = new();
        public List<AiMealItem> Meals { get; set; } = new();
    }

    public class AiMealItem
    {
        public string MealType { get; set; } = string.Empty;
        public int RecipeId { get; set; }
        public string RecipeName { get; set; } = string.Empty;
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }
        public double Price { get; set; }
        public double ServingMultiplier { get; set; } = 1.0;  // 0.5x, 1x, 1.5x
    }

    // ===================================================
    // DTOs cho Generate Instructions (Gemini + Fallback)
    // ===================================================

    public class AiIngredientInput
    {
        public string Name { get; set; } = string.Empty;
        public double QuantityGrams { get; set; }
        public int Role { get; set; } = 1;  // 1=Chính, 2=Phụ, 3=Gia vị
    }

    public class AiInstructionsRequest
    {
        public string RecipeName { get; set; } = string.Empty;
        public List<AiIngredientInput> Ingredients { get; set; } = new();
    }

    public class AiInstructionsResult
    {
        public string Status { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;  // "gemini" or "fallback"
        public List<string> Steps { get; set; } = new();
    }
}
