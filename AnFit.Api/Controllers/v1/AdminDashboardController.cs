using AnFit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AnFit.Api.Controllers.v1
{
    [Authorize(Roles = "Admin")]
    [Route("api/v1/admin/dashboard")]
    public class AdminDashboardController : BaseApiController
    {
        private readonly IUserService _userService;

        public AdminDashboardController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Thống kê tổng quan cho Admin Dashboard
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var result = await _userService.GetAdminDashboardAsync();
            return Ok(new { success = true, data = result });
        }
    }
}
