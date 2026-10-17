using AnFit.Api.Models.Entities;
using System;
using System.Threading.Tasks;

namespace AnFit.Api.Repositories.Interfaces
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);

        // Thu hồi tất cả refresh token của 1 user (dùng khi đổi mật khẩu, xóa tài khoản...)
        Task RevokeAllByUserIdAsync(Guid userId);
    }
}