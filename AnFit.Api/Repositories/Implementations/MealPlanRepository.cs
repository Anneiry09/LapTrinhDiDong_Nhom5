using AnFit.Api.Data;
using AnFit.Api.Models.Entities;
using AnFit.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Implementations
{
    public class MealPlanRepository : Repository<MealPlan>, IMealPlanRepository
    {
        public MealPlanRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<MealPlan?> GetByUserAndDateAsync(Guid userId, DateOnly planDate)
        {
            return await _dbSet.FirstOrDefaultAsync(m => m.UserId == userId && m.PlanDate == planDate);
        }

        public async Task<MealPlan?> GetWithDetailsAsync(Guid userId, DateOnly planDate)
        {
            // Dùng Include để Entity Framework tự động JOIN bảng MealPlanDetails và FoodItems
            return await _dbSet
                .Include(m => m.MealPlanDetails)
                    .ThenInclude(md => md.FoodItem)
                .Include(m => m.MealPlanDetails)
                    .ThenInclude(md => md.Recipe)
                .FirstOrDefaultAsync(m => m.UserId == userId && m.PlanDate == planDate);
        }

        public async Task AddDetailAsync(MealPlanDetail detail)
        {
            await _context.Set<MealPlanDetail>().AddAsync(detail);
        }

        public async Task<System.Collections.Generic.IEnumerable<MealPlan>> GetRoadmapWithDetailsAsync(Guid userId, DateOnly startDate, DateOnly endDate)
        {
            return await _dbSet
                .Include(m => m.MealPlanDetails)
                    .ThenInclude(md => md.FoodItem)
                .Include(m => m.MealPlanDetails)
                    .ThenInclude(md => md.Recipe)
                .Where(m => m.UserId == userId && m.PlanDate >= startDate && m.PlanDate <= endDate)
                .ToListAsync();
        }

        public async Task<MealPlanDetail?> GetDetailWithFoodAsync(int detailId)
        {
            return await _context.Set<MealPlanDetail>()
                .Include(d => d.FoodItem)
                .Include(d => d.Recipe)
                .Include(d => d.MealPlan)
                .FirstOrDefaultAsync(d => d.Id == detailId);
        }

        public async Task<MealPlanDetail?> GetDetailByIdAsync(int detailId)
        {
            return await _context.Set<MealPlanDetail>()
                .FirstOrDefaultAsync(d => d.Id == detailId);
        }
    }
}