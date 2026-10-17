using AnFit.Api.Data;
using AnFit.Api.Models.Entities;
using AnFit.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Implementations
{
    public class FoodItemRepository : Repository<FoodItem>, IFoodItemRepository
    {
        public FoodItemRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<FoodItem>> SearchByNameAsync(string searchTerm)
        {
            // Kiểm tra null trước khi gọi Contains vì Name là nullable
            return await _dbSet.Where(f => f.Name != null && f.Name.Contains(searchTerm))
                               .ToListAsync();
        }

        public async Task<IEnumerable<FoodItem>> GetSafeForConditionsAsync(IEnumerable<int> conditionIds)
        {
            // Lấy tất cả FoodItem KHÔNG liên kết với bất kỳ MedicalCondition nào trong danh sách
            var conditionIdList = conditionIds.ToList();

            return await _dbSet
                .Where(f => !f.Conditions.Any(c => conditionIdList.Contains(c.Id)))
                .ToListAsync();
        }
    }
}