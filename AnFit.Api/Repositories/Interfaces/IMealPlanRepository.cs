using AnFit.Api.Models.Entities;
using System;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Interfaces
{
    public interface IMealPlanRepository : IRepository<MealPlan>
    {
        // Lấy kế hoạch ăn uống của 1 user trong 1 ngày cụ thể (không kèm chi tiết)
        Task<MealPlan?> GetByUserAndDateAsync(Guid userId, DateOnly planDate);

        // Lấy kế hoạch ăn uống kèm theo chi tiết các món ăn (Include MealPlanDetails + FoodItem)
        Task<MealPlan?> GetWithDetailsAsync(Guid userId, DateOnly planDate);

        // Thêm chi tiết thực đơn vào CSDL
        Task AddDetailAsync(MealPlanDetail detail);

        // Lấy lộ trình dinh dưỡng 30 ngày kèm chi tiết (MealPlanDetails + FoodItem)
        Task<System.Collections.Generic.IEnumerable<MealPlan>> GetRoadmapWithDetailsAsync(Guid userId, DateOnly startDate, DateOnly endDate);

        // Lấy chi tiết thực đơn kèm FoodItem + MealPlan (dùng cho Swap)
        Task<MealPlanDetail?> GetDetailWithFoodAsync(int detailId);

        // Lấy chi tiết thực đơn theo Id (dùng cho Track)
        Task<MealPlanDetail?> GetDetailByIdAsync(int detailId);
    }
}