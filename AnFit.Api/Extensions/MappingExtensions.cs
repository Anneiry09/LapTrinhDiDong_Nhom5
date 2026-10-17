using AnFit.Api.DTOs.Responses;
using AnFit.Api.Models.Entities;

namespace AnFit.Api.Extensions
{
    public static class MappingExtensions
    {
        public static UserProfileResponse ToResponse(this User user)
        {
            return new UserProfileResponse
            {
                Id = user.Id,
                // Dùng ?? string.Empty để phòng trường hợp chuỗi bị null
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,

                // Cung cấp giá trị mặc định (0 hoặc 1) nếu dữ liệu dưới DB là null
                Role = user.Role ?? 1,
                Gender = user.Gender ?? 0,
                HeightCm = user.HeightCm ?? 0.0,
                DateOfBirth = user.DateOfBirth, // Biến này bên DTO đã là DateTime? nên map thẳng được
                TargetGoal = user.TargetGoal ?? 0,
                DailyBudget = user.DailyBudget ?? 0m, // 0m là cách khai báo số 0 kiểu decimal
                IsCompensateExercise = user.IsCompensateExercise ?? true
            };
        }

        public static MedicalConditionResponse ToResponse(this MedicalCondition condition)
        {
            return new MedicalConditionResponse
            {
                Id = condition.Id,
                ConditionName = condition.ConditionName ?? string.Empty,

                // Kiểm tra xem entity của bạn cột Type có phải là int? không, nếu có thì thêm ?? 0
                Type = condition.Type ?? 0
            };
        }
        public static ExerciseResponse ToResponse(this Exercise exercise)
        {
            return new ExerciseResponse
            {
                Id = exercise.Id,
                Name = exercise.Name ?? string.Empty,
                MuscleGroup = exercise.MuscleGroup ?? string.Empty,
                BaseCaloriesBurned = exercise.BaseCaloriesBurned ?? 0,
                InstructionUrl = exercise.InstructionUrl
            };
        }

        public static BodyLogResponse ToResponse(this UserBodyLog log)
        {
            return new BodyLogResponse
            {
                Id = log.Id,
                Weight = log.Weight.HasValue ? System.Math.Round(log.Weight.Value, 1) : 0,
                BodyFatPercentage = log.BodyFatPercentage.HasValue ? System.Math.Round(log.BodyFatPercentage.Value, 1) : null,
                Bmi = log.Bmi.HasValue ? System.Math.Round(log.Bmi.Value, 1) : null,
                RecordedAt = log.RecordedAt ?? System.DateTime.UtcNow
            };
        }

        public static DailyHealthLogResponse ToResponse(this DailyHealthLog log)
        {
            return new DailyHealthLogResponse
            {
                Id = log.Id,
                RecordDate = log.RecordDate ?? DateOnly.FromDateTime(System.DateTime.UtcNow.AddHours(7)),
                Tdee = log.CalculatedTdee ?? 0,
                CaloriesConsumed = log.CaloriesConsumed, 
                CaloriesBurned = null, // Dự kiến tính từ WorkoutPlan
                ActiveCalories = log.ActiveCalories,
                StepCount = log.TotalSteps,
                SleepDurationMinutes = log.SleepDurationMinutes,
                RestingHeartRate = log.RestingHeartRate,
                MaxHeartRate = log.MaxHeartRate
            };
        }

        public static FoodItemResponse ToResponse(this FoodItem food)
        {
            return new FoodItemResponse
            {
                Id = food.Id,
                Name = food.Name ?? string.Empty,
                CaloriesPerUnit = food.CaloriesPerUnit ?? 0,
                ProteinPerUnit = food.ProteinPerUnit ?? 0,
                CarbsPerUnit = food.CarbsPerUnit ?? 0,
                FatPerUnit = food.FatPerUnit ?? 0,
                EstimatedPrice = food.EstimatedPrice ?? 0,
                Unit = food.Unit ?? string.Empty,
                Category = food.Category ?? 0,
                // Lấy tên các bệnh lý dị ứng với món này (nếu có)
                HealthWarnings = food.Conditions?.Select(c => c.ConditionName ?? string.Empty).ToList()
            };
        }

        public static MealPlanDetailResponse ToResponse(this MealPlanDetail detail)
        {
            var food = detail.FoodItem;
            double qty = detail.Quantity ?? 1.0;
            double ratio = qty / 100.0;

            return new MealPlanDetailResponse
            {
                Id = detail.Id,
                MealType = detail.MealType ?? 0,
                ServingSize = qty,
                CalculatedCalories = detail.CalculatedCalories ?? (food?.CaloriesPerUnit ?? 0) * ratio,
                CalculatedProtein = detail.CalculatedProtein ?? (food?.ProteinPerUnit ?? 0) * ratio,
                CalculatedCarbs = detail.CalculatedCarbs ?? (food?.CarbsPerUnit ?? 0) * ratio,
                CalculatedFat = detail.CalculatedFat ?? (food?.FatPerUnit ?? 0) * ratio,
                CalculatedPrice = detail.CalculatedPrice ?? 0,
                IsTracked = detail.IsTracked,
                RecipeId = detail.RecipeId,
                RecipeName = detail.Recipe?.Name,
                FoodItem = food?.ToResponse()
            };
        }

        public static MealPlanResponse ToResponse(this MealPlan plan)
        {
            // Parse BudgetWarning (stored as newline-separated text) into list
            var warnings = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrWhiteSpace(plan.BudgetWarning))
            {
                warnings.AddRange(plan.BudgetWarning.Split('\n', System.StringSplitOptions.RemoveEmptyEntries));
            }

            // Calculate BudgetUsedPercent from User's DailyBudget if available
            decimal dailyBudget = plan.User?.DailyBudget ?? 0;
            double budgetUsedPercent = dailyBudget > 0 && plan.TotalPrice.HasValue
                ? (double)(plan.TotalPrice.Value / dailyBudget * 100)
                : 0;

            var planDate = plan.PlanDate ?? DateOnly.FromDateTime(System.DateTime.UtcNow.AddHours(7));

            // Tính toán động trạng thái hoàn thành dựa trên chi tiết các bữa ăn đã được ghi nhận (Tracked)
            int status = plan.Status ?? 0;
            var details = plan.MealPlanDetails?.ToList() ?? new System.Collections.Generic.List<MealPlanDetail>();
            if (details.Count > 0 && details.All(d => d.IsTracked == true))
            {
                status = 1; // Hoàn thành tất cả món ăn trong ngày (được ghi nhận trong roadmap là tích xanh)
            }
            else
            {
                // Nếu là ngày trong quá khứ và chưa hoàn thành hết
                var today = DateOnly.FromDateTime(System.DateTime.UtcNow.AddHours(7));
                if (planDate < today && status != 1)
                {
                    status = 3; // Trạng thái "Bỏ qua / Chưa hoàn thành"
                }
            }

            return new MealPlanResponse
            {
                Id = plan.Id,
                PlanDate = planDate,
                TotalCalories = plan.TotalCalories ?? 0,
                TotalProtein = plan.TotalProtein ?? 0,
                TotalCarbs = plan.TotalCarbs ?? 0,
                TotalFat = plan.TotalFat ?? 0,
                TotalPrice = plan.TotalPrice ?? 0,
                Status = status,
                UserRating = plan.UserRating,
                DailyBudget = dailyBudget,
                BudgetUsedPercent = System.Math.Round(budgetUsedPercent, 1),
                BudgetWarnings = warnings,
                MealPlanDetails = plan.MealPlanDetails?.Select(d => d.ToResponse()).ToList() ?? new System.Collections.Generic.List<MealPlanDetailResponse>()
            };
        }

        public static WorkoutActivityResponse ToResponse(this WorkoutActivity activity)
        {
            var isWarmup = activity.RestTimeSeconds == -10;
            var isCooldown = activity.RestTimeSeconds == -20;

            return new WorkoutActivityResponse
            {
                Id = activity.Id,
                Sets = isWarmup || isCooldown ? 0 : (activity.SetsCount ?? 0),
                RepsPerSet = isWarmup || isCooldown ? 0 : (activity.RepsPerSet ?? 0),
                RestTimeSeconds = isWarmup || isCooldown ? 0 : (activity.RestTimeSeconds ?? 0),
                DurationMinutes = isWarmup || isCooldown ? 10.0 : 8.0,
                EstimatedCaloriesBurned = isWarmup || isCooldown 
                    ? (activity.Exercise?.BaseCaloriesBurned ?? 0) * (10.0 / 10.0) 
                    : (activity.Exercise?.BaseCaloriesBurned ?? 0),
                Phase = isWarmup ? "Warm-up" : isCooldown ? "Cool-down" : "Core Workout",
                Exercise = activity.Exercise?.ToResponse()
            };
        }

        public static WorkoutPlanResponse ToResponse(this WorkoutPlan plan)
        {
            int status = plan.Status ?? 0;
            var planDate = plan.PlanDate ?? DateOnly.FromDateTime(System.DateTime.UtcNow.AddHours(7));
            var today = DateOnly.FromDateTime(System.DateTime.UtcNow.AddHours(7));

            // Nếu ngày trong quá khứ mà trạng thái vẫn là chưa tập (Status = 0)
            if (status == 0 && planDate < today)
            {
                status = 3; // Bỏ qua / Chưa hoàn thành
            }

            return new WorkoutPlanResponse
            {
                Id = plan.Id,
                PlanDate = planDate,
                TotalEstimatedCaloriesBurn = plan.EstimatedCaloriesBurn ?? 0,
                DifficultyLevel = plan.DifficultyRating ?? 0,
                Status = status,
                UserRating = plan.DifficultyRating, // Dùng tạm cột này để đại diện rate
                WorkoutActivities = plan.WorkoutActivities?.Select(a => a.ToResponse()).ToList() ?? new System.Collections.Generic.List<WorkoutActivityResponse>()
            };
        }
    }
}