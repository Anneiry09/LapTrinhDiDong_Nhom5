using AnFit.Api.DTOs.Requests;
using AnFit.Api.DTOs.Responses;
using System.Threading.Tasks;

namespace AnFit.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> RegisterWithOnboardingAsync(RegisterWithOnboardingRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task RevokeTokenAsync(string token);
        Task SendOtpRegisterAsync(string email);
        // Thêm 4 hàm này vào dưới hàm RevokeTokenAsync
        Task<string?> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request);
        Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
        Task DeleteAccountAsync(Guid userId);
    }
}