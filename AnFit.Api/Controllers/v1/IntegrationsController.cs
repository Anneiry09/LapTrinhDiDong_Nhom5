using AnFit.Api.DTOs.Requests;
using AnFit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AnFit.Api.Controllers.v1
{
    [Authorize]
    public class IntegrationsController : BaseApiController
    {
        private readonly IGarminSyncService _garminService;

        public IntegrationsController(IGarminSyncService garminService)
        {
            _garminService = garminService;
        }

        [HttpPost("garmin/connect")]
        public async Task<IActionResult> ConnectGarmin([FromQuery] string callbackUrl)
        {
            var userId = GetCurrentUserId();
            // Lấy link để Frontend bật popup chuyển hướng người dùng sang Garmin
            var authUrl = await _garminService.InitiateConnectionAsync(userId, callbackUrl);

            return Ok(new { success = true, data = new { authUrl } });
        }

        [HttpPost("garmin/callback")]
        public async Task<IActionResult> GarminCallback([FromBody] GarminCallbackRequest request)
        {
            var userId = GetCurrentUserId();
            await _garminService.HandleCallbackAsync(userId, request);

            return Ok(new { success = true, message = "Kết nối Garmin thành công!" });
        }

        [HttpPost("garmin/sync")]
        public async Task<IActionResult> SyncGarminData([FromBody] SyncHealthDataRequest request)
        {
            var userId = GetCurrentUserId();

            // Đồng bộ dữ liệu từng ngày trong khoảng thời gian request yêu cầu
            for (var date = request.StartDate; date <= request.EndDate; date = date.AddDays(1))
            {
                await _garminService.SyncDailyDataAsync(userId, date);
            }

            return Ok(new { success = true, message = "Đồng bộ dữ liệu Garmin thành công." });
        }

        /// <summary>
        /// Lấy trạng thái liên kết Garmin hiện tại.
        /// </summary>
        [HttpGet("garmin/status")]
        public async Task<IActionResult> GetGarminStatus()
        {
            var userId = GetCurrentUserId();
            var status = await _garminService.GetIntegrationStatusAsync(userId);
            return Ok(new { success = true, data = status });
        }

        public class GarminBridgeConnectRequest
        {
            public string Email { get; set; } = null!;
            public string Password { get; set; } = null!;
        }

        /// <summary>
        /// Kết nối Garmin qua Bridge mode (Python microservice).
        /// Dùng khi chưa có Garmin Developer API key chính thức.
        /// </summary>
        [HttpPost("garmin/connect-bridge")]
        public async Task<IActionResult> ConnectGarminBridge([FromBody] GarminBridgeConnectRequest request)
        {
            var userId = GetCurrentUserId();
            await _garminService.ConnectBridgeAsync(userId, request.Email, request.Password);

            return Ok(new { success = true, message = "Kết nối Garmin qua Bridge thành công!" });
        }

        [HttpDelete("garmin/disconnect")]
        public async Task<IActionResult> DisconnectGarmin()
        {
            var userId = GetCurrentUserId();
            await _garminService.DisconnectAsync(userId);
            return Ok(new { success = true, message = "Đã ngắt kết nối với Garmin." });
        }
    }
}