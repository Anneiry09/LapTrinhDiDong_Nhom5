using AnFit.Api.DTOs.Responses;
using AnFit.Api.Extensions;
using AnFit.Api.Repositories.Interfaces;
using AnFit.Api.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Implementations.Core
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardSummaryResponse> GetSummaryAsync(Guid userId)
        {
            var vnNow = DateTime.UtcNow.AddHours(7); // Múi giờ Việt Nam (UTC+7)
            var today = DateOnly.FromDateTime(vnNow);

            // 1. Lấy cân nặng hiện tại (bản ghi mới nhất)
            var latestBodyLog = await _unitOfWork.UserBodyLogs.GetLatestByUserAsync(userId);
            double currentWeight = latestBodyLog?.Weight ?? 0;

            // 2. Lấy Nhật ký Sức khỏe (TDEE, Calo, Bước chân...) hôm nay
            var healthLogs = await _unitOfWork.DailyHealthLogs.GetByUserAndDateRangeAsync(userId, today, today);
            var todayHealthLog = healthLogs.FirstOrDefault();
            double todayTdee = todayHealthLog?.CalculatedTdee ?? 0;

            // 3. Lấy Thực đơn hôm nay
            var todayMealPlan = await _unitOfWork.MealPlans.GetWithDetailsAsync(userId, today);

            // 4. Lấy Lịch tập hôm nay
            var todayWorkoutPlan = await _unitOfWork.WorkoutPlans.GetWithActivitiesAsync(userId, today);

            // Trả về DTO tổng hợp
            return new DashboardSummaryResponse
            {
                CurrentWeight = currentWeight,
                TodayTdee = todayTdee,
                TodayHealthStatus = todayHealthLog?.ToResponse(),
                TodayMealPlan = todayMealPlan?.ToResponse(),
                TodayWorkoutPlan = todayWorkoutPlan?.ToResponse()
            };
        }
    }
}