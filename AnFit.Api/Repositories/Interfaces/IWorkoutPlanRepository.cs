using AnFit.Api.Models.Entities;
using System;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Interfaces
{
    public interface IWorkoutPlanRepository : IRepository<WorkoutPlan>
    {
        // Lấy kế hoạch tập luyện của 1 user trong 1 ngày (không kèm chi tiết)
        Task<WorkoutPlan?> GetByUserAndDateAsync(Guid userId, DateOnly planDate);

        // Lấy kế hoạch tập luyện kèm danh sách WorkoutActivities + Exercise
        Task<WorkoutPlan?> GetWithActivitiesAsync(Guid userId, DateOnly planDate);

        // Lấy lộ trình kế hoạch tập luyện trong khoảng ngày kèm danh sách WorkoutActivities + Exercise
        Task<System.Collections.Generic.IEnumerable<WorkoutPlan>> GetRoadmapWithActivitiesAsync(Guid userId, DateOnly startDate, DateOnly endDate);

        // Thêm 1 WorkoutActivity vào DB (dùng khi AI tạo danh sách bài tập)
        Task AddActivityAsync(WorkoutActivity activity);
    }
}