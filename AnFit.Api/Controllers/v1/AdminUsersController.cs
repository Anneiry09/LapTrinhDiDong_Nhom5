using AnFit.Api.DTOs.Requests;
using AnFit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AnFit.Api.Controllers.v1
{
    [Authorize(Roles = "Admin")]
    [Route("api/v1/admin/users")]
    public class AdminUsersController : BaseApiController
    {
        private readonly IUserService _userService;

        public AdminUsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Danh sách tất cả Users (phân trang) — Admin only
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] PaginationQuery query)
        {
            var result = await _userService.GetAllUsersAsync(query);
            return Ok(new { success = true, data = result.Data, pagination = new { result.PageNumber, result.PageSize, result.TotalRecords, result.TotalPages } });
        }

        /// <summary>
        /// Chi tiết 1 User theo ID — Admin only
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var result = await _userService.GetUserByIdAsync(id);
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// Ban / Unban User — Admin only
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> ToggleUserStatus(Guid id, [FromBody] ToggleStatusRequest request)
        {
            await _userService.ToggleUserStatusAsync(id, request.IsDeleted);
            var message = request.IsDeleted ? "Đã vô hiệu hóa tài khoản." : "Đã kích hoạt lại tài khoản.";
            return Ok(new { success = true, message });
        }
    }

    // DTO nhỏ inline vì chỉ dùng ở đây
    public class ToggleStatusRequest
    {
        public bool IsDeleted { get; set; }
    }
}
