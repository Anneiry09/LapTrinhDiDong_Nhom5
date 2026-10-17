using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AnFit.Api.Exceptions;
using AnFit.Api.DTOs.Responses;

namespace AnFit.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Cho phép request đi tiếp vào Controllers
                await _next(context);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi ném ra, tóm ngay tại đây
                _logger.LogError(ex, "Đã xảy ra lỗi hệ thống.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = (int)HttpStatusCode.InternalServerError;
            var response = new ApiResponse<object>("Lỗi máy chủ nội bộ. Vui lòng thử lại sau.");

            // Nếu là lỗi mình chủ động ném ra (Custom Exceptions ở bước 1.2)
            if (exception is ApiException apiException)
            {
                statusCode = apiException.StatusCode;
                response = new ApiResponse<object>(apiException.Message, apiException.Errors);
            }

            context.Response.StatusCode = statusCode;

            // Chuyển object thành JSON (viết hoa chữ cái đầu theo chuẩn CamelCase của JS)
            var result = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            return context.Response.WriteAsync(result);
        }
    }
}