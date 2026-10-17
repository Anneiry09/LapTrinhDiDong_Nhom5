using AnFit.Api.DTOs.Requests;
using AnFit.Api.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Interfaces
{
    public interface IDailyHealthLogService
    {
        Task<DailyHealthLogResponse> GetTodayLogAsync(Guid userId);
        Task<DailyHealthLogResponse> UpdateTodayLogAsync(Guid userId, UpdateDailyHealthLogRequest request);
        Task<IEnumerable<DailyHealthLogResponse>> GetByDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate);
        Task<DailyHealthLogResponse> SyncHealthConnectAsync(Guid userId, SyncHealthConnectRequest request);
    }
}