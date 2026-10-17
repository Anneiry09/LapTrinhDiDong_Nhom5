using AnFit.Api.DTOs.Requests;
using AnFit.Api.DTOs.Responses;
using System;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Interfaces
{
    public interface IMealPlanService
    {
        Task<MealPlanResponse?> GetByDateAsync(Guid userId, DateOnly date);
        Task<System.Collections.Generic.IEnumerable<MealPlanResponse>> GetMealRoadmapAsync(Guid userId, DateOnly? startDate);
        Task<MealPlanResponse> GeneratePlanAsync(Guid userId, GenerateMealPlanRequest request);
        Task Generate30DayMealRoadmapAsync(Guid userId, bool force = false);
        Task UpdateStatusAsync(int planId, UpdatePlanStatusRequest request);
        Task RatePlanAsync(int planId, RatePlanRequest request);
        Task<MealPlanDetailResponse> SwapMealItemAsync(int detailId, int newRecipeId);
        Task TrackMealAsync(int detailId);
    }
}