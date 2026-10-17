using AnFit.Api.DTOs.Requests;
using AnFit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AnFit.Api.Controllers.v1
{
    [Authorize]
    public class DailyHealthLogsController : BaseApiController
    {
        private readonly IDailyHealthLogService _healthLogService;

        public DailyHealthLogsController(IDailyHealthLogService healthLogService)
        {
            _healthLogService = healthLogService;
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetToday()
        {
            var userId = GetCurrentUserId();
            var response = await _healthLogService.GetTodayLogAsync(userId);
            return Ok(new { success = true, data = response });
        }

        [HttpPut("today")]
        public async Task<IActionResult> UpdateToday([FromBody] UpdateDailyHealthLogRequest request)
        {
            var userId = GetCurrentUserId();
            var response = await _healthLogService.UpdateTodayLogAsync(userId, request);
            return Ok(new { success = true, message = "Cập nhật sức khỏe hôm nay thành công.", data = response });
        }

        [HttpGet]
        public async Task<IActionResult> GetByDateRange(
            [FromQuery] DateOnly? startDate, 
            [FromQuery] DateOnly? endDate,
            [FromQuery(Name = "from")] DateOnly? from,
            [FromQuery(Name = "to")] DateOnly? to)
        {
            var start = from ?? startDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-7));
            var end = to ?? endDate ?? DateOnly.FromDateTime(DateTime.Today);
            
            var userId = GetCurrentUserId();
            var response = await _healthLogService.GetByDateRangeAsync(userId, start, end);
            return Ok(new { success = true, data = response });
        }

        [HttpPost("sync-health-connect")]
        public async Task<IActionResult> SyncHealthConnect([FromBody] SyncHealthConnectRequest request)
        {
            var userId = GetCurrentUserId();
            var response = await _healthLogService.SyncHealthConnectAsync(userId, request);
            return Ok(new { success = true, message = "Đồng bộ dữ liệu sức khỏe cục bộ thành công.", data = response });
        }
    }
}