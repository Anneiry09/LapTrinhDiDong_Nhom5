using AnFit.Api.DTOs.Requests;
using AnFit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AnFit.Api.Controllers.v1
{
    [Authorize] // Bắt buộc đăng nhập
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            var profile = await _userService.GetProfileAsync(userId);
            return Ok(new { success = true, data = profile });
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = GetCurrentUserId();
            var updatedProfile = await _userService.UpdateProfileAsync(userId, request);
            return Ok(new { success = true, message = "Cập nhật hồ sơ thành công.", data = updatedProfile });
        }

        [HttpPut("me/medical-conditions")]
        public async Task<IActionResult> UpdateMedicalConditions([FromBody] UpdateMedicalConditionsRequest request)
        {
            var userId = GetCurrentUserId();
            await _userService.UpdateMedicalConditionsAsync(userId, request);
            return Ok(new { success = true, message = "Cập nhật bệnh lý thành công." });
        }

        // API này chỉ dành cho Admin (Role = 2 ở DB, lúc tạo Token ta map thành chuỗi "Admin")
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserProfileById(Guid id)
        {
            var profile = await _userService.GetProfileAsync(id);
            return Ok(new { success = true, data = profile });
        }
    }
}