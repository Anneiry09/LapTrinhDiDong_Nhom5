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
    public class DailyHealthLogService : IDailyHealthLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DailyHealthLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DailyHealthLogResponse> GetTodayLogAsync(Guid userId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
            var logs = await _unitOfWork.DailyHealthLogs.GetByUserAndDateRangeAsync(userId, today, today);
            var log = logs.FirstOrDefault();

            if (log == null)
            {
                // Nếu hôm nay chưa có Log, tạo tự động 1 cái mới và tính TDEE
                log = await CreateLogForTodayAsync(userId, today);
            }

            return log.ToResponse();
        }

        public async Task<DailyHealthLogResponse> UpdateTodayLogAsync(Guid userId, UpdateDailyHealthLogRequest request)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
            var logs = await _unitOfWork.DailyHealthLogs.GetByUserAndDateRangeAsync(userId, today, today);
            var log = logs.FirstOrDefault() ?? await CreateLogForTodayAsync(userId, today);

            // Cập nhật các chỉ số
            // if (request.CaloriesConsumed.HasValue) log.CaloriesConsumed = request.CaloriesConsumed; // (Cột này chưa có trong Database, tạm bỏ qua hoặc dùng MealPlan)
            if (request.ActiveCalories.HasValue) log.ActiveCalories = request.ActiveCalories;
            if (request.StepCount.HasValue) log.TotalSteps = request.StepCount;
            if (request.SleepDurationMinutes.HasValue) log.SleepDurationMinutes = request.SleepDurationMinutes;
            if (request.RestingHeartRate.HasValue) log.RestingHeartRate = request.RestingHeartRate;

            log.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return log.ToResponse();
        }

        public async Task<IEnumerable<DailyHealthLogResponse>> GetByDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate)
        {
            var logs = await _unitOfWork.DailyHealthLogs.GetByUserAndDateRangeAsync(userId, startDate, endDate);
            return logs.Select(x => x.ToResponse());
        }

        public async Task<DailyHealthLogResponse> SyncHealthConnectAsync(Guid userId, SyncHealthConnectRequest request)
        {
            // 1. Tìm bản ghi theo UserId + ngày cụ thể (upsert by date)
            var logs = await _unitOfWork.DailyHealthLogs.GetByUserAndDateRangeAsync(userId, request.TargetDate, request.TargetDate);
            var log = logs.FirstOrDefault();

            if (log == null)
            {
                // Tạo bản ghi mới cho ngày này
                double tdee = 2000;
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                var latestBody = await _unitOfWork.UserBodyLogs.GetLatestByUserAsync(userId);

                if (user != null && latestBody != null && user.HeightCm > 0 && user.DateOfBirth.HasValue)
                {
                    int age = DateTime.UtcNow.Year - user.DateOfBirth.Value.Year;
                    double weight = latestBody.Weight ?? 60;
                    double height = user.HeightCm ?? 160;

                    double bmr = (10 * weight) + (6.25 * height) - (5 * age);
                    bmr = user.Gender == 1 ? bmr + 5 : bmr - 161;
                    tdee = bmr * 1.2;
                }

                log = new DailyHealthLog
                {
                    UserId = userId,
                    RecordDate = request.TargetDate,
                    CalculatedTdee = Math.Round(tdee, 2),
                    ExternalSourceId = "HealthConnect",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.DailyHealthLogs.AddAsync(log);
            }

            // 2. Cập nhật dữ liệu sức khỏe
            log.TotalSteps = request.TotalSteps;
            log.ActiveCalories = request.ActiveCalories;
            log.RestingHeartRate = request.RestingHeartRate > 0 ? request.RestingHeartRate : log.RestingHeartRate;
            log.MaxHeartRate = request.MaxHeartRate > 0 ? request.MaxHeartRate : log.MaxHeartRate;
            log.SleepDurationMinutes = request.SleepDurationMinutes > 0 ? request.SleepDurationMinutes : log.SleepDurationMinutes;
            log.StressLevel = request.StressLevel > 0 ? (int?)Math.Round(request.StressLevel) : log.StressLevel;
            log.ExternalSourceId = "HealthConnect";
            log.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return log.ToResponse();
        }

        // --- HÀM PHỤ TRỢ: Tạo Log và tính TDEE ---
        private async Task<DailyHealthLog> CreateLogForTodayAsync(Guid userId, DateOnly date)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            var latestBody = await _unitOfWork.UserBodyLogs.GetLatestByUserAsync(userId);

            double tdee = 2000; // Mặc định nếu thiếu thông tin

            if (user != null && latestBody != null && user.HeightCm > 0 && user.DateOfBirth.HasValue)
            {
                // Tính TDEE theo công thức Mifflin-St Jeor
                int age = DateTime.UtcNow.Year - user.DateOfBirth.Value.Year;
                double weight = latestBody.Weight ?? 60;
                double height = user.HeightCm ?? 160;

                double bmr = (10 * weight) + (6.25 * height) - (5 * age);

                // Nam (1) cộng 5, Nữ (2) trừ 161
                bmr = user.Gender == 1 ? bmr + 5 : bmr - 161;

                // TDEE = BMR * Hệ số vận động (Mặc định lấy 1.2 - Ít vận động)
                tdee = bmr * 1.2;
            }

            var newLog = new DailyHealthLog
            {
                UserId = userId,
                RecordDate = date,
                CalculatedTdee = Math.Round(tdee, 2),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.DailyHealthLogs.AddAsync(newLog);
            await _unitOfWork.SaveChangesAsync();

            return newLog;
        }
    }
}