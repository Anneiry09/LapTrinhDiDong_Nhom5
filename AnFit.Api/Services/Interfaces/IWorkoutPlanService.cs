using AnFit.Api.DTOs.Requests;
using AnFit.Api.DTOs.Responses;
using System;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Interfaces
{
    public interface IWorkoutPlanService
    {
        Task<WorkoutPlanResponse?> GetByDateAsync(Guid userId, DateOnly date);
        Task<System.Collections.Generic.IEnumerable<WorkoutPlanResponse>> GetRoadmapAsync(Guid userId, DateOnly? startDate);
        Task<WorkoutPlanResponse> GeneratePlanAsync(Guid userId, GenerateWorkoutPlanRequest request);
        Task UpdateStatusAsync(int planId, UpdatePlanStatusRequest request);
        Task RatePlanAsync(int planId, RatePlanRequest request);
        Task Generate30DayRoadmapAsync(Guid userId, bool force = false);
    }
}