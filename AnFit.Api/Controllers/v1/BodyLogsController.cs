using AnFit.Api.DTOs.Requests;
using AnFit.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AnFit.Api.Controllers.v1
{
    [Authorize]
    public class BodyLogsController : BaseApiController
    {
        private readonly IUserBodyLogService _bodyLogService;

        public BodyLogsController(IUserBodyLogService bodyLogService)
        {
            _bodyLogService = bodyLogService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBodyLogRequest request)
        {
            var userId = GetCurrentUserId();
            var response = await _bodyLogService.CreateAsync(userId, request);
            return Ok(new { success = true, message = "Ghi nhận chỉ số cơ thể thành công.", data = response });
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest()
        {
            var userId = GetCurrentUserId();
            var response = await _bodyLogService.GetLatestAsync(userId);
            return Ok(new { success = true, data = response });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] PaginationQuery query)
        {
            var userId = GetCurrentUserId();
            var response = await _bodyLogService.GetHistoryAsync(userId, query);

            // Hàm này trả về thẳng response vì ở Service ta đã gói nó trong PagedResponse<T> rồi
            return Ok(response);
        }
    }
}