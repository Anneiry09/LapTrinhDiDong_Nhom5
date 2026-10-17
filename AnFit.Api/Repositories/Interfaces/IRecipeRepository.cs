using AnFit.Api.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Interfaces
{
    public interface IRecipeRepository : IRepository<Recipe>
    {
        /// <summary>
        /// Lấy tất cả các món ăn an toàn (không chứa bất kỳ nguyên liệu nào liên quan đến các MedicalCondition trong list cấm).
        /// Query được dịch thành SQL NOT EXISTS / NOT IN giúp lọc trực tiếp ở Database để tối ưu hiệu năng.
        /// </summary>
        Task<IEnumerable<Recipe>> GetSafeRecipesForUserAsync(IEnumerable<int> conditionIds);

        /// <summary>
        /// Lấy chi tiết Recipe kèm theo các nguyên liệu và bệnh lý liên kết để hỗ trợ check an toàn.
        /// </summary>
        Task<Recipe?> GetByIdWithIngredientsAsync(int id);
    }
}
