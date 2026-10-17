using System;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {

        // Hàm quan trọng nhất: Lưu tất cả thay đổi từ các Repo vào Database cùng lúc
        Task<int> SaveChangesAsync();

        // Các hàm quản lý Transaction (Dùng cho các nghiệp vụ phức tạp)
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        IUserRepository Users { get; }
        IMealPlanRepository MealPlans { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        IDailyHealthLogRepository DailyHealthLogs { get; }
        IExerciseRepository Exercises { get; }
        IFoodItemRepository FoodItems { get; }
        IWorkoutPlanRepository WorkoutPlans { get; }
        IUserBodyLogRepository UserBodyLogs { get; }
        IUserIntegrationRepository UserIntegrations { get; }
        IMedicalConditionRepository MedicalConditions { get; }
        IRepository<AnFit.Api.Models.Entities.ActivityCategory> ActivityCategories { get; }
        IRecipeRepository Recipes { get; }
        IRepository<AnFit.Api.Models.Entities.DailyProgressRecord> DailyProgressRecords { get; }
    }
}