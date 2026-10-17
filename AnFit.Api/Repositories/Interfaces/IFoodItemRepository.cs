using AnFit.Api.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Interfaces
{
    public interface IFoodItemRepository : IRepository<FoodItem>
    {
        Task<IEnumerable<FoodItem>> SearchByNameAsync(string searchTerm);

        // Lấy danh sách thực phẩm an toàn cho user có bệnh lý — loại trừ FoodItem liên kết với các MedicalCondition trong danh sách
        Task<IEnumerable<FoodItem>> GetSafeForConditionsAsync(IEnumerable<int> conditionIds);
    }
}