using AnFit.Api.DTOs.Requests;
using AnFit.Api.DTOs.Responses;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Interfaces
{
    public interface IFoodItemService
    {
        Task<PagedResponse<System.Collections.Generic.IEnumerable<FoodItemResponse>>> GetAllAsync(FoodItemFilterQuery query);
        Task<FoodItemResponse?> GetByIdAsync(int id);
        Task<FoodItemResponse> CreateAsync(CreateFoodItemRequest request);
        Task<FoodItemResponse> UpdateAsync(int id, UpdateFoodItemRequest request);
        Task DeleteAsync(int id);
    }
}