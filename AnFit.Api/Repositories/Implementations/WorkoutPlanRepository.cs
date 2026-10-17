using AnFit.Api.Data;
using AnFit.Api.Models.Entities;
using AnFit.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Implementations
{
    public class WorkoutPlanRepository : Repository<WorkoutPlan>, IWorkoutPlanRepository
    {
        public WorkoutPlanRepository(ApplicationDbContext context) : base(context) { }

        public async Task<WorkoutPlan?> GetByUserAndDateAsync(Guid userId, DateOnly planDate)
        {
            return await _dbSet.FirstOrDefaultAsync(w => w.UserId == userId && w.PlanDate == planDate);
        }

        public async Task<WorkoutPlan?> GetWithActivitiesAsync(Guid userId, DateOnly planDate)
        {
            return await _dbSet
                .Include(w => w.WorkoutActivities)
                    .ThenInclude(wa => wa.Exercise)
                .FirstOrDefaultAsync(w => w.UserId == userId && w.PlanDate == planDate);
        }

        public async Task<System.Collections.Generic.IEnumerable<WorkoutPlan>> GetRoadmapWithActivitiesAsync(Guid userId, DateOnly startDate, DateOnly endDate)
        {
            return await _dbSet
                .Include(w => w.WorkoutActivities)
                    .ThenInclude(wa => wa.Exercise)
                .Where(w => w.UserId == userId && w.PlanDate >= startDate && w.PlanDate <= endDate)
                .ToListAsync();
        }

        public async Task AddActivityAsync(WorkoutActivity activity)
        {
            await _context.Set<WorkoutActivity>().AddAsync(activity);
        }
    }
}