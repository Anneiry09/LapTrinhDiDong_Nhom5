using AnFit.Api.DTOs.Requests;
using AnFit.Api.DTOs.Responses;
using AnFit.Api.Exceptions;
using AnFit.Api.Extensions;
using AnFit.Api.Models.Entities;
using AnFit.Api.Repositories.Interfaces;
using AnFit.Api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Implementations.Core
{
    public class MealPlanService : IMealPlanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDailyHealthLogService _healthLogService;
        private readonly IAiService _aiService;

        public MealPlanService(IUnitOfWork unitOfWork, IDailyHealthLogService healthLogService, IAiService aiService)
        {
            _unitOfWork = unitOfWork;
            _healthLogService = healthLogService;
            _aiService = aiService;
        }

        public async Task<MealPlanResponse?> GetByDateAsync(Guid userId, DateOnly date)
        {
            var plan = await _unitOfWork.MealPlans.GetWithDetailsAsync(userId, date);
            return plan?.ToResponse();
        }

        public async Task<MealPlanResponse> GeneratePlanAsync(Guid userId, GenerateMealPlanRequest request)
        {
            // Xóa kế hoạch cũ trong ngày (nếu có) để tạo mới
            var existingPlan = await _unitOfWork.MealPlans.GetWithDetailsAsync(userId, request.PlanDate);
            if (existingPlan != null)
            {
                _unitOfWork.MealPlans.Remove(existingPlan);
                await _unitOfWork.SaveChangesAsync();
            }

            // 1. Lấy TDEE của ngày hôm đó nếu TargetCalories null
            double targetCalories = request.TargetCalories ?? 0;
            if (targetCalories == 0)
            {
                var healthLog = await _healthLogService.GetTodayLogAsync(userId);
                targetCalories = healthLog?.Tdee ?? 2000; // Mặc định 2000 nếu chưa có log
            }

            // 2. Lấy thông tin User cùng danh sách Bệnh lý / Dị ứng thức ăn
            var user = await _unitOfWork.Users.GetWithDetailsAsync(userId);
            if (user == null) throw new NotFoundException("Không tìm thấy người dùng.");

            decimal budget = request.BudgetLimit ?? user.DailyBudget ?? 0m;

            // 3. Lấy tất cả Recipes AN TOÀN (lọc trực tiếp tại Database để tối ưu hiệu năng)
            var userConditionIds = user.Conditions.Select(c => c.Id).ToList();
            var safeRecipes = await _unitOfWork.Recipes.GetSafeRecipesForUserAsync(userConditionIds);
            var recipeList = safeRecipes.ToList();

            if (recipeList.Count == 0)
                throw new BadRequestException("Không tìm thấy công thức món ăn nào an toàn với sức khỏe và dị ứng của bạn.");

            // 4. GỌI AI MICROSERVICE (Python) — gửi recipes
            var aiRequest = new AiMealPlanRequest
            {
                Tdee = targetCalories,
                TargetGoal = user.TargetGoal ?? 2,
                DailyBudget = (double)budget,
                AvailableRecipes = recipeList.Select(r => new AiAvailableRecipe
                {
                    Id = r.Id,
                    Name = r.Name ?? "",
                    Calories = r.TotalCalories ?? 0,
                    Protein = r.TotalProtein ?? 0,
                    Carbs = r.TotalCarbs ?? 0,
                    Fat = r.TotalFat ?? 0,
                    Price = (double)(r.EstimatedPrice ?? 0),
                    MealSuitability = r.MealSuitability ?? 6,
                    Category = r.Category ?? 1
                }).ToList()
            };

            var aiResult = await _aiService.GenerateMealPlanAsync(aiRequest);

            // 5. TẠO VỎ KẾ HOẠCH
            var budgetWarning = aiResult.Warnings.Count > 0
                ? string.Join("\n", aiResult.Warnings)
                : null;

            var newPlan = new MealPlan
            {
                UserId = userId,
                PlanDate = request.PlanDate,
                TotalCalories = 0,
                TotalProtein = 0,
                TotalCarbs = 0,
                TotalFat = 0,
                TotalPrice = 0,
                Status = 0,
                BudgetWarning = budgetWarning,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.MealPlans.AddAsync(newPlan);
            await _unitOfWork.SaveChangesAsync();

            // 6. LƯU CHI TIẾT TỪ KẾT QUẢ CỦA AI (Recipe-based)
            foreach (var aiMeal in aiResult.Meals)
            {
                var recipe = recipeList.FirstOrDefault(r => r.Id == aiMeal.RecipeId);
                if (recipe == null) continue;

                int mealType = aiMeal.MealType switch
                {
                    "Breakfast" => 1,
                    "Lunch" => 2,
                    "Dinner" => 3,
                    _ => 4
                };

                var detail = new MealPlanDetail
                {
                    MealPlanId = newPlan.Id,
                    RecipeId = recipe.Id,
                    MealType = mealType,
                    Quantity = aiMeal.ServingMultiplier * 100, // Lưu dưới dạng gram tương đối
                    CalculatedCalories = aiMeal.Calories,
                    CalculatedProtein = aiMeal.Protein,
                    CalculatedCarbs = aiMeal.Carbs,
                    CalculatedFat = aiMeal.Fat,
                    CalculatedPrice = (decimal)aiMeal.Price
                };

                await _unitOfWork.MealPlans.AddDetailAsync(detail);

                newPlan.TotalCalories += aiMeal.Calories;
                newPlan.TotalProtein += aiMeal.Protein;
                newPlan.TotalCarbs += aiMeal.Carbs;
                newPlan.TotalFat += aiMeal.Fat;
                newPlan.TotalPrice += (decimal)aiMeal.Price;
            }

            _unitOfWork.MealPlans.Update(newPlan);
            await _unitOfWork.SaveChangesAsync();

            var result = await _unitOfWork.MealPlans.GetWithDetailsAsync(userId, request.PlanDate);
            return result!.ToResponse();
        }

        public async Task UpdateStatusAsync(int planId, UpdatePlanStatusRequest request)
        {
            var plan = await _unitOfWork.MealPlans.GetByIdAsync(planId);
            if (plan == null) throw new NotFoundException("Không tìm thấy thực đơn.");

            plan.Status = request.Status;
            plan.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.MealPlans.Update(plan);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RatePlanAsync(int planId, RatePlanRequest request)
        {
            var plan = await _unitOfWork.MealPlans.GetByIdAsync(planId);
            if (plan == null) throw new NotFoundException("Không tìm thấy thực đơn.");

            plan.UserRating = request.Rating;
            plan.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.MealPlans.Update(plan);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<MealPlanResponse>> GetMealRoadmapAsync(Guid userId, DateOnly? startDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
            var queryDate = startDate ?? today;

            var plans = await _unitOfWork.MealPlans.FindAsync(m => m.UserId == userId);
            var maxPlan = plans.OrderByDescending(m => m.PlanDate).FirstOrDefault();
            if (maxPlan != null && maxPlan.PlanDate.HasValue)
            {
                var calculatedStartDate = maxPlan.PlanDate.Value.AddDays(-29);
                if (!startDate.HasValue || (startDate.Value >= calculatedStartDate && startDate.Value <= maxPlan.PlanDate.Value))
                {
                    queryDate = calculatedStartDate;
                }
            }

            var endDate = queryDate.AddDays(29);
            var plansList = await _unitOfWork.MealPlans.GetRoadmapWithDetailsAsync(userId, queryDate, endDate);
            return plansList.Select(p => p.ToResponse()).OrderBy(p => p.PlanDate);
        }

        public async Task Generate30DayMealRoadmapAsync(Guid userId, bool force = false)
        {
            // 1. Lấy thông tin User
            var user = await _unitOfWork.Users.GetWithDetailsAsync(userId);
            if (user == null) throw new NotFoundException("Không tìm thấy người dùng.");

            // Check xem lộ trình hiện tại đã hoàn thành 100% hay chưa
            var plans = await _unitOfWork.MealPlans.FindAsync(m => m.UserId == userId);
            var maxPlan = plans.OrderByDescending(m => m.PlanDate).FirstOrDefault();
            if (maxPlan != null && maxPlan.PlanDate.HasValue)
            {
                var activeStartDate = maxPlan.PlanDate.Value.AddDays(-29);
                var activeEndDate = maxPlan.PlanDate.Value;
                
                var activePlansList = await _unitOfWork.MealPlans.GetRoadmapWithDetailsAsync(userId, activeStartDate, activeEndDate);
                var activePlans = activePlansList.ToList();

                // Điều kiện hoàn thành 100%: phải có đủ 30 ngày và tất cả món ăn trong ngày đều được ăn (IsTracked = true)
                bool isCompleted100 = activePlans.Count >= 30 && activePlans.All(p => 
                    p.MealPlanDetails != null && 
                    p.MealPlanDetails.Count > 0 && 
                    p.MealPlanDetails.All(d => d.IsTracked == true)
                );

                if (!isCompleted100 && !force)
                {
                    throw new BadRequestException("ROADMAP_INCOMPLETE");
                }
            }

            // 2. Tính TDEE (lấy từ HealthLog gần nhất, default 2000)
            var healthLog = await _healthLogService.GetTodayLogAsync(userId);
            double baseTdee = healthLog?.Tdee ?? 2000;

            // 3. Lấy tất cả Recipes AN TOÀN (lọc trực tiếp tại Database theo danh sách bệnh lý/dị ứng để tối ưu hiệu năng)
            var userConditionIds = user.Conditions.Select(c => c.Id).ToList();
            var safeRecipes = await _unitOfWork.Recipes.GetSafeRecipesForUserAsync(userConditionIds);
            var recipeList = safeRecipes.ToList();

            if (recipeList.Count == 0) return;

            decimal budget = user.DailyBudget ?? 0m;
            int targetGoal = (int)(user.TargetGoal ?? 2);

            // 4. Xóa lộ trình cũ (nếu có)
            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
            for (int i = 0; i < 30; i++)
            {
                var targetDate = today.AddDays(i);
                var existingPlan = await _unitOfWork.MealPlans.GetWithDetailsAsync(userId, targetDate);
                if (existingPlan != null)
                {
                    _unitOfWork.MealPlans.Remove(existingPlan);
                }
            }
            await _unitOfWork.SaveChangesAsync();

            // 5. Chuẩn bị danh sách recipes cho AI
            var aiRecipeList = recipeList.Select(r => new AiAvailableRecipe
            {
                Id = r.Id,
                Name = r.Name ?? "",
                Calories = r.TotalCalories ?? 0,
                Protein = r.TotalProtein ?? 0,
                Carbs = r.TotalCarbs ?? 0,
                Fat = r.TotalFat ?? 0,
                Price = (double)(r.EstimatedPrice ?? 0),
                MealSuitability = r.MealSuitability ?? 6,
                Category = r.Category ?? 1
            }).ToList();

            // 6. Loop 30 ngày — tracking recent recipes (Repetition Penalty)
            // Rolling window: giữ recipe IDs của 2 ngày gần nhất
            var recentDaysWindow = new Queue<List<int>>();

            for (int i = 0; i < 30; i++)
            {
                var targetDate = today.AddDays(i);

                // Flatten window → danh sách IDs gần đây
                var recentIds = new List<int>();
                foreach (var dayIds in recentDaysWindow)
                    recentIds.AddRange(dayIds);

                var aiRequest = new AiMealPlanRequest
                {
                    Tdee = baseTdee,
                    TargetGoal = targetGoal,
                    DailyBudget = (double)budget,
                    AvailableRecipes = aiRecipeList,
                    RecentRecipeIds = recentIds
                };

                AiMealPlanResult aiResult;
                try
                {
                    aiResult = await _aiService.GenerateMealPlanAsync(aiRequest);
                }
                catch
                {
                    continue;
                }

                // Thu thập recipe IDs của ngày này → đưa vào window
                var todayRecipeIds = aiResult.Meals.Select(m => m.RecipeId).ToList();
                recentDaysWindow.Enqueue(todayRecipeIds);
                if (recentDaysWindow.Count > 2) // Giữ tối đa 2 ngày
                    recentDaysWindow.Dequeue();

                // 7. Tạo MealPlan cho ngày này
                var budgetWarn = aiResult.Warnings.Count > 0
                    ? string.Join("\n", aiResult.Warnings)
                    : null;

                var newPlan = new MealPlan
                {
                    UserId = userId,
                    PlanDate = targetDate,
                    TotalCalories = 0,
                    TotalProtein = 0,
                    TotalCarbs = 0,
                    TotalFat = 0,
                    TotalPrice = 0,
                    Status = 0,
                    BudgetWarning = budgetWarn,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.MealPlans.AddAsync(newPlan);
                await _unitOfWork.SaveChangesAsync();

                // 8. Lưu chi tiết từ kết quả AI (Recipe-based)
                foreach (var aiMeal in aiResult.Meals)
                {
                    var recipe = recipeList.FirstOrDefault(r => r.Id == aiMeal.RecipeId);
                    if (recipe == null) continue;

                    int mealType = aiMeal.MealType switch
                    {
                        "Breakfast" => 1,
                        "Lunch" => 2,
                        "Dinner" => 3,
                        _ => 4
                    };

                    var detail = new MealPlanDetail
                    {
                        MealPlanId = newPlan.Id,
                        RecipeId = recipe.Id,
                        MealType = mealType,
                        Quantity = aiMeal.ServingMultiplier * 100,
                        CalculatedCalories = aiMeal.Calories,
                        CalculatedProtein = aiMeal.Protein,
                        CalculatedCarbs = aiMeal.Carbs,
                        CalculatedFat = aiMeal.Fat,
                        CalculatedPrice = (decimal)aiMeal.Price
                    };

                    await _unitOfWork.MealPlans.AddDetailAsync(detail);

                    newPlan.TotalCalories += aiMeal.Calories;
                    newPlan.TotalProtein += aiMeal.Protein;
                    newPlan.TotalCarbs += aiMeal.Carbs;
                    newPlan.TotalFat += aiMeal.Fat;
                    newPlan.TotalPrice += (decimal)aiMeal.Price;
                }

                _unitOfWork.MealPlans.Update(newPlan);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<MealPlanDetailResponse> SwapMealItemAsync(int detailId, int newRecipeId)
        {
            // 1. Tìm detail hiện tại
            var detail = await _unitOfWork.MealPlans.GetDetailWithFoodAsync(detailId);
            if (detail == null) throw new NotFoundException("Không tìm thấy chi tiết thực đơn.");

            // 2. Tìm recipe mới cùng các nguyên liệu của nó
            var newRecipe = await _unitOfWork.Recipes.GetByIdWithIngredientsAsync(newRecipeId);
            if (newRecipe == null) throw new NotFoundException("Không tìm thấy công thức món ăn.");

            // Kiểm tra tính an toàn của món ăn mới đối với dị ứng & bệnh lý của người dùng
            var userId = detail.MealPlan?.UserId ?? Guid.Empty;
            var user = await _unitOfWork.Users.GetWithDetailsAsync(userId);
            if (user != null)
            {
                var userConditionIds = user.Conditions.Select(c => c.Id).ToList();
                if (userConditionIds.Count > 0)
                {
                    bool isUnsafe = newRecipe.Ingredients.Any(ri => 
                        ri.FoodItem.Conditions.Any(c => userConditionIds.Contains(c.Id))
                    );

                    if (isUnsafe)
                    {
                        throw new BadRequestException("Món ăn này chứa nguyên liệu gây dị ứng hoặc không an toàn với bệnh lý của bạn.");
                    }
                }
            }

            // 3. Kiểm tra MealSuitability — chỉ cho swap recipe phù hợp cùng bữa
            int mealBit = detail.MealType switch
            {
                1 => 1, // Breakfast
                2 => 2, // Lunch
                3 => 4, // Dinner
                _ => 8  // Snack
            };
            if ((newRecipe.MealSuitability ?? 0 & mealBit) == 0)
                throw new BadRequestException("Món ăn này không phù hợp với bữa ăn hiện tại.");

            // 4. Cập nhật detail
            double oldCal = detail.CalculatedCalories ?? 0;
            double oldPro = detail.CalculatedProtein ?? 0;
            double oldCarbs = detail.CalculatedCarbs ?? 0;
            double oldFat = detail.CalculatedFat ?? 0;
            decimal oldPrice = detail.CalculatedPrice ?? 0;

            detail.RecipeId = newRecipeId;
            detail.FoodItemId = null; // Clear FoodItem reference
            detail.CalculatedCalories = newRecipe.TotalCalories ?? 0;
            detail.CalculatedProtein = newRecipe.TotalProtein ?? 0;
            detail.CalculatedCarbs = newRecipe.TotalCarbs ?? 0;
            detail.CalculatedFat = newRecipe.TotalFat ?? 0;
            detail.CalculatedPrice = newRecipe.EstimatedPrice ?? 0;

            // 5. Cập nhật tổng MealPlan
            var plan = detail.MealPlan;
            if (plan != null)
            {
                plan.TotalCalories = (plan.TotalCalories ?? 0) - oldCal + (detail.CalculatedCalories ?? 0);
                plan.TotalProtein = (plan.TotalProtein ?? 0) - oldPro + (detail.CalculatedProtein ?? 0);
                plan.TotalCarbs = (plan.TotalCarbs ?? 0) - oldCarbs + (detail.CalculatedCarbs ?? 0);
                plan.TotalFat = (plan.TotalFat ?? 0) - oldFat + (detail.CalculatedFat ?? 0);
                plan.TotalPrice = (plan.TotalPrice ?? 0) - oldPrice + (detail.CalculatedPrice ?? 0);
                plan.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.MealPlans.Update(plan);
            }

            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.MealPlans.GetDetailWithFoodAsync(detailId);
            return updated!.ToResponse();
        }

        public async Task TrackMealAsync(int detailId)
        {
            var detail = await _unitOfWork.MealPlans.GetDetailByIdAsync(detailId);
            if (detail == null) throw new NotFoundException("Không tìm thấy chi tiết thực đơn.");

            detail.IsTracked = true;
            await _unitOfWork.SaveChangesAsync();
        }
    }
}