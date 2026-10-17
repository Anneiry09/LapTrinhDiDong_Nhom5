using AnFit.Api.DTOs.Responses;
using System;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryResponse> GetSummaryAsync(Guid userId);
    }
}