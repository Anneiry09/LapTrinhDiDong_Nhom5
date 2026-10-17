using AnFit.Api.DTOs.Requests;
using AnFit.Api.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileResponse> GetProfileAsync(Guid userId);
        Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
        Task UpdateMedicalConditionsAsync(Guid userId, UpdateMedicalConditionsRequest request);
        Task DeleteAccountAsync(Guid userId); // Soft Delete
        Task RestoreAccountAsync(Guid userId); // Admin khôi phục tài khoản

        // --- Admin ---
        Task<PagedResponse<List<AdminUserListResponse>>> GetAllUsersAsync(PaginationQuery query);
        Task<UserProfileResponse> GetUserByIdAsync(Guid userId);
        Task ToggleUserStatusAsync(Guid userId, bool isDeleted);
        Task<AdminDashboardResponse> GetAdminDashboardAsync();
    }
}