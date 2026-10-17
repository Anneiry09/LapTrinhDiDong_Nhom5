using AnFit.Api.DTOs.Requests;
using AnFit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AnFit.Api.Controllers.v1
{
    // Controller này sẽ có route là: /api/v1/Auth
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var response = await _authService.RegisterAsync(request);
            // Trả về format JSON đồng nhất
            return Ok(new { success = true, message = "Đăng ký thành công.", data = response });
        }

        [HttpPost("send-otp-register")]
        public async Task<IActionResult> SendOtpRegister([FromBody] SendOtpRequest request)
        {
            await _authService.SendOtpRegisterAsync(request.Email);
            return Ok(new { success = true, message = "Mã xác nhận đã được gửi đến email của bạn." });
        }

        [HttpPost("register-with-onboarding")]
        public async Task<IActionResult> RegisterWithOnboarding([FromBody] RegisterWithOnboardingRequest request)
        {
            var response = await _authService.RegisterWithOnboardingAsync(request);
            return CreatedAtAction(nameof(RegisterWithOnboarding), new { success = true, message = "Đăng ký với Onboarding thành công.", data = response });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            return Ok(new { success = true, message = "Đăng nhập thành công.", data = response });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var response = await _authService.RefreshTokenAsync(request);
            return Ok(new { success = true, message = "Làm mới Token thành công.", data = response });
        }

        [Authorize] // Bắt buộc phải có Token mới được gọi API này
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            await _authService.RevokeTokenAsync(request.RefreshToken);
            return Ok(new { success = true, message = "Đã thu hồi Token." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await _authService.ForgotPasswordAsync(request);
            return Ok(new { success = true, message = "Mã OTP đã được gửi về Gmail của bạn. Vui lòng kiểm tra và nhập mã để đặt lại mật khẩu." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            await _authService.ResetPasswordAsync(request);
            return Ok(new { success = true, message = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại." });
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = GetCurrentUserId(); // Dùng hàm tiện ích từ BaseApiController
            await _authService.ChangePasswordAsync(userId, request);
            return Ok(new { success = true, message = "Đổi mật khẩu thành công." });
        }

        [Authorize]
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = GetCurrentUserId();
            await _authService.DeleteAccountAsync(userId);
            return Ok(new { success = true, message = "Tài khoản của bạn đã được xóa thành công." });
        }
    }
}