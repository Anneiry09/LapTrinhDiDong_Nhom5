using AnFit.Api.Data;
using AnFit.Api.Models.Entities;
using AnFit.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Implementations
{
    public class RecipeRepository : Repository<Recipe>, IRecipeRepository
    {
        public RecipeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Recipe>> GetSafeRecipesForUserAsync(IEnumerable<int> conditionIds)
        {
            var conditionIdList = conditionIds.ToList();

            // Nếu người dùng không có bệnh lý hoặc dị ứng nào, trả về toàn bộ danh sách để tối ưu hiệu năng
            if (conditionIdList.Count == 0)
            {
                return await _dbSet.ToListAsync();
            }

            // Thực hiện query lọc ngay từ SQL Server:
            // Lấy các Recipes mà KHÔNG có bất kỳ nguyên liệu (Ingredients) nào 
            // có FoodItem liên kết với bất kỳ MedicalCondition nào trong danh sách cấm của User.
            return await _dbSet
                .Where(r => !r.Ingredients.Any(ri => 
                    ri.FoodItem.Conditions.Any(c => conditionIdList.Contains(c.Id))
                ))
                .ToListAsync();
        }

        public async Task<Recipe?> GetByIdWithIngredientsAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Ingredients)
                    .ThenInclude(ri => ri.FoodItem)
                        .ThenInclude(f => f.Conditions)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
