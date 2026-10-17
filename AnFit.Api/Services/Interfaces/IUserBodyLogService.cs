using AnFit.Api.DTOs.Requests;
using AnFit.Api.DTOs.Responses;
using System;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Interfaces
{
    public interface IUserBodyLogService
    {
        Task<BodyLogResponse> CreateAsync(Guid userId, CreateBodyLogRequest request);
        Task<BodyLogResponse?> GetLatestAsync(Guid userId);
        Task<PagedResponse<System.Collections.Generic.IEnumerable<BodyLogResponse>>> GetHistoryAsync(Guid userId, PaginationQuery query);
    }
}