using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace AnFit.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        // Hàm tiện ích giúp các Controller con dễ dàng lấy ID của User đang gửi request
        protected Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Không thể xác thực người dùng. Vui lòng đăng nhập lại.");
        }
    }
}