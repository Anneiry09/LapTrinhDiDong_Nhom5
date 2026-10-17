using AnFit.Api.DTOs.Requests;
using AnFit.Api.DTOs.Responses;
using AnFit.Api.Models.Entities;
using AnFit.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnFit.Api.Controllers.v1
{
    [Authorize]
    public class ProgressController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProgressController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost("complete-day")]
        public async Task<IActionResult> CompleteDay([FromBody] CompleteDayRequest request)
        {
            var userId = GetCurrentUserId();
            var targetDate = request.Date;

            // 1. Dinh dưỡng (MealPlan & Details)
            // Lấy MealPlan của ngày hôm đó kèm theo MealPlanDetails
            var mealPlan = await _unitOfWork.MealPlans.GetWithDetailsAsync(userId, targetDate);
            double totalCaloriesConsumed = 0;
            double totalProtein = 0;
            double totalCarbs = 0;
            double totalFat = 0;

            if (mealPlan != null)
            {
                // Chỉ tính các món đã check "Đã ăn" (IsTracked = true)
                var trackedMeals = mealPlan.MealPlanDetails.Where(md => md.IsTracked).ToList();
                totalCaloriesConsumed = trackedMeals.Sum(md => md.CalculatedCalories ?? 0);
                totalProtein = trackedMeals.Sum(md => md.CalculatedProtein ?? 0);
                totalCarbs = trackedMeals.Sum(md => md.CalculatedCarbs ?? 0);
                totalFat = trackedMeals.Sum(md => md.CalculatedFat ?? 0);
            }

            // 2. Tập luyện & Vận động
            var workoutPlan = await _unitOfWork.WorkoutPlans.GetWithActivitiesAsync(userId, targetDate);
            string workoutPlanName = "Vận động tự do";
            double totalCaloriesBurned = 0;

            if (workoutPlan != null)
            {
                totalCaloriesBurned = workoutPlan.EstimatedCaloriesBurn ?? 0;
                
                // Xác định tên buổi tập dựa trên các bài tập trong buổi
                var activities = workoutPlan.WorkoutActivities.ToList();
                if (activities.Any())
                {
                    // Lấy ra các nhóm cơ hoặc bài tập nổi bật
                    var exerciseNames = activities
                        .Where(a => a.Exercise != null && !string.IsNullOrEmpty(a.Exercise.Name))
                        .Select(a => a.Exercise!.Name)
                        .Take(2)
                        .ToList();

                    var muscleGroups = activities
                        .Where(a => a.Exercise != null && !string.IsNullOrEmpty(a.Exercise.MuscleGroup))
                        .Select(a => a.Exercise!.MuscleGroup)
                        .Distinct()
                        .Take(2)
                        .ToList();

                    if (muscleGroups.Any())
                    {
                        workoutPlanName = "Tập " + string.Join(" & ", muscleGroups);
                    }
                    else if (exerciseNames.Any())
                    {
                        workoutPlanName = string.Join(" + ", exerciseNames);
                    }
                    else
                    {
                        workoutPlanName = "Tập luyện thể chất";
                    }
                }
            }

            // Lấy thêm Steps & ActiveCalories từ DailyHealthLog (nếu có đồng bộ từ Health Connect/Garmin)
            int totalSteps = 0;
            var healthLogs = await _unitOfWork.DailyHealthLogs.GetByUserAndDateRangeAsync(userId, targetDate, targetDate);
            var todayHealthLog = healthLogs.FirstOrDefault();
            if (todayHealthLog != null)
            {
                totalSteps = todayHealthLog.TotalSteps ?? 0;
                
                // Nếu calo vận động từ Health Connect lớn hơn trong workout plan thì lấy từ Health Connect
                var activeCal = todayHealthLog.ActiveCalories ?? 0;
                if (activeCal > totalCaloriesBurned)
                {
                    totalCaloriesBurned = activeCal;
                }
            }

            // 3. Chỉ số cơ thể lúc đó (Cân nặng mới nhất tính đến thời điểm chốt sổ)
            double? currentWeight = null;
            // Tìm body log gần nhất từ trước đến ngày này
            var targetDateTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, 23, 59, 59);
            var bodyLogs = await _unitOfWork.UserBodyLogs.FindAsync(b => b.UserId == userId && b.RecordedAt <= targetDateTime);
            var latestBodyLog = bodyLogs
                .OrderByDescending(b => b.RecordedAt)
                .FirstOrDefault();

            if (latestBodyLog != null)
            {
                currentWeight = latestBodyLog.Weight;
            }

            // 4. Lưu hoặc Cập nhật DailyProgressRecord
            var progressRecords = await _unitOfWork.DailyProgressRecords.FindAsync(p => p.UserId == userId && p.RecordDate == targetDate);
            var record = progressRecords.FirstOrDefault();
            bool isNew = record == null;

            if (isNew)
            {
                record = new DailyProgressRecord
                {
                    UserId = userId,
                    RecordDate = targetDate,
                    CreatedAt = DateTime.UtcNow
                };
            }

            record.TotalCaloriesConsumed = totalCaloriesConsumed;
            record.TotalProtein = totalProtein;
            record.TotalCarbs = totalCarbs;
            record.TotalFat = totalFat;
            record.WorkoutPlanName = workoutPlanName;
            record.TotalCaloriesBurned = totalCaloriesBurned;
            record.TotalSteps = totalSteps;
            record.CurrentWeight = currentWeight;
            record.UpdatedAt = DateTime.UtcNow;

            if (isNew)
            {
                await _unitOfWork.DailyProgressRecords.AddAsync(record);
            }
            else
            {
                _unitOfWork.DailyProgressRecords.Update(record);
            }

            await _unitOfWork.SaveChangesAsync();

            return Ok(new { success = true, message = "Hoàn thành ngày thành công!", data = DailyProgressResponse.FromEntity(record) });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int month, [FromQuery] int year)
        {
            var userId = GetCurrentUserId();
            
            // Tự động chốt sổ cho tất cả các ngày đã qua trong tháng chưa có bản ghi
            try
            {
                await EnsureProgressRecordsForPastDaysAsync(userId, month, year);
            }
            catch (Exception ex)
            {
                // Ghi nhận lỗi nhưng không chặn trả về danh sách lịch sử hiện có
                Console.WriteLine($"[WARN] Lỗi tự động chốt sổ lịch sử: {ex.Message}");
            }

            // Tìm tất cả các bản ghi có RecordDate trong tháng và năm chỉ định
            var records = await _unitOfWork.DailyProgressRecords.FindAsync(p => 
                p.UserId == userId && 
                p.RecordDate != null && 
                p.RecordDate.Value.Month == month && 
                p.RecordDate.Value.Year == year
            );

            var responseList = records
                .OrderBy(p => p.RecordDate)
                .Select(DailyProgressResponse.FromEntity)
                .ToList();

            return Ok(new { success = true, data = responseList });
        }

        private async Task EnsureProgressRecordsForPastDaysAsync(Guid userId, int month, int year)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)); // Timezone GMT+7
            
            // Tìm các ngày trong tháng/năm đã qua (bé hơn today)
            int daysInMonth = DateTime.DaysInMonth(year, month);
            var pastDates = new List<DateOnly>();
            
            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateOnly(year, month, day);
                if (date < today)
                {
                    pastDates.Add(date);
                }
            }

            if (!pastDates.Any()) return;

            // Lấy các bản ghi hiện có
            var existingRecords = await _unitOfWork.DailyProgressRecords.FindAsync(p =>
                p.UserId == userId &&
                p.RecordDate != null &&
                p.RecordDate.Value.Month == month &&
                p.RecordDate.Value.Year == year
            );
            
            var existingDates = existingRecords
                .Where(p => p.RecordDate.HasValue)
                .Select(p => p.RecordDate.Value)
                .ToHashSet();

            var missingDates = pastDates.Where(d => !existingDates.Contains(d)).ToList();

            if (!missingDates.Any()) return;

            foreach (var targetDate in missingDates)
            {
                // 1. Dinh dưỡng (MealPlan & Details)
                var mealPlan = await _unitOfWork.MealPlans.GetWithDetailsAsync(userId, targetDate);
                double totalCaloriesConsumed = 0;
                double totalProtein = 0;
                double totalCarbs = 0;
                double totalFat = 0;

                if (mealPlan != null)
                {
                    var trackedMeals = mealPlan.MealPlanDetails.Where(md => md.IsTracked).ToList();
                    totalCaloriesConsumed = trackedMeals.Sum(md => md.CalculatedCalories ?? 0);
                    totalProtein = trackedMeals.Sum(md => md.CalculatedProtein ?? 0);
                    totalCarbs = trackedMeals.Sum(md => md.CalculatedCarbs ?? 0);
                    totalFat = trackedMeals.Sum(md => md.CalculatedFat ?? 0);
                }

                // 2. Tập luyện & Vận động
                var workoutPlan = await _unitOfWork.WorkoutPlans.GetWithActivitiesAsync(userId, targetDate);
                string workoutPlanName = "Vận động tự do";
                double totalCaloriesBurned = 0;

                if (workoutPlan != null)
                {
                    totalCaloriesBurned = workoutPlan.EstimatedCaloriesBurn ?? 0;
                    var activities = workoutPlan.WorkoutActivities.ToList();
                    if (activities.Any())
                    {
                        var exerciseNames = activities
                            .Where(a => a.Exercise != null && !string.IsNullOrEmpty(a.Exercise.Name))
                            .Select(a => a.Exercise!.Name)
                            .Take(2)
                            .ToList();

                        var muscleGroups = activities
                            .Where(a => a.Exercise != null && !string.IsNullOrEmpty(a.Exercise.MuscleGroup))
                            .Select(a => a.Exercise!.MuscleGroup)
                            .Distinct()
                            .Take(2)
                            .ToList();

                        if (muscleGroups.Any())
                        {
                            workoutPlanName = "Tập " + string.Join(" & ", muscleGroups);
                        }
                        else if (exerciseNames.Any())
                        {
                            workoutPlanName = string.Join(" + ", exerciseNames);
                        }
                        else
                        {
                            workoutPlanName = "Tập luyện thể chất";
                        }
                    }
                }

                // Lấy thêm Steps & ActiveCalories từ DailyHealthLog (nếu có đồng bộ từ Health Connect/Garmin)
                int totalSteps = 0;
                var healthLogs = await _unitOfWork.DailyHealthLogs.GetByUserAndDateRangeAsync(userId, targetDate, targetDate);
                var healthLog = healthLogs.FirstOrDefault();
                if (healthLog != null)
                {
                    totalSteps = healthLog.TotalSteps ?? 0;
                    var activeCal = healthLog.ActiveCalories ?? 0;
                    if (activeCal > totalCaloriesBurned)
                    {
                        totalCaloriesBurned = activeCal;
                    }
                }

                // 3. Chỉ số cơ thể lúc đó (Cân nặng mới nhất tính đến thời điểm chốt sổ)
                double? currentWeight = null;
                var targetDateTime = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, 23, 59, 59);
                var bodyLogs = await _unitOfWork.UserBodyLogs.FindAsync(b => b.UserId == userId && b.RecordedAt <= targetDateTime);
                var latestBodyLog = bodyLogs
                    .OrderByDescending(b => b.RecordedAt)
                    .FirstOrDefault();

                if (latestBodyLog != null)
                {
                    currentWeight = latestBodyLog.Weight;
                }

                // 4. Lưu DailyProgressRecord
                var record = new DailyProgressRecord
                {
                    UserId = userId,
                    RecordDate = targetDate,
                    TotalCaloriesConsumed = totalCaloriesConsumed,
                    TotalProtein = totalProtein,
                    TotalCarbs = totalCarbs,
                    TotalFat = totalFat,
                    WorkoutPlanName = workoutPlanName,
                    TotalCaloriesBurned = totalCaloriesBurned,
                    TotalSteps = totalSteps,
                    CurrentWeight = currentWeight,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.DailyProgressRecords.AddAsync(record);
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
