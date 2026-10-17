using AnFit.Api.Data;
using AnFit.Api.Models.Entities;
using AnFit.Api.Repositories.Interfaces;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        // Dùng nullable vì lazy-load pattern (??=) — khởi tạo khi cần
        private IUserRepository? _users;
        private IMealPlanRepository? _mealPlans;
        private IRefreshTokenRepository? _refreshTokens;
        private IDailyHealthLogRepository? _dailyHealthLogs;
        private IExerciseRepository? _exercises;
        private IFoodItemRepository? _foodItems;
        private IWorkoutPlanRepository? _workoutPlans;
        private IUserBodyLogRepository? _userBodyLogs;
        private IUserIntegrationRepository? _userIntegrations;
        private IMedicalConditionRepository? _medicalConditions;
        private IRepository<ActivityCategory>? _activityCategories;
        private IRecipeRepository? _recipes;
        private IRepository<DailyProgressRecord>? _dailyProgressRecords;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IMealPlanRepository MealPlans => _mealPlans ??= new MealPlanRepository(_context);
        public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context);
        public IDailyHealthLogRepository DailyHealthLogs => _dailyHealthLogs ??= new DailyHealthLogRepository(_context);
        public IExerciseRepository Exercises => _exercises ??= new ExerciseRepository(_context);
        public IFoodItemRepository FoodItems => _foodItems ??= new FoodItemRepository(_context);
        public IWorkoutPlanRepository WorkoutPlans => _workoutPlans ??= new WorkoutPlanRepository(_context);
        public IUserBodyLogRepository UserBodyLogs => _userBodyLogs ??= new UserBodyLogRepository(_context);
        public IUserIntegrationRepository UserIntegrations => _userIntegrations ??= new UserIntegrationRepository(_context);
        public IMedicalConditionRepository MedicalConditions => _medicalConditions ??= new MedicalConditionRepository(_context);
        public IRepository<ActivityCategory> ActivityCategories => _activityCategories ??= new Repository<ActivityCategory>(_context);
        public IRecipeRepository Recipes => _recipes ??= new RecipeRepository(_context);
        public IRepository<DailyProgressRecord> DailyProgressRecords => _dailyProgressRecords ??= new Repository<DailyProgressRecord>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}