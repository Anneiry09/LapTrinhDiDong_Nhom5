using System.Collections.Generic;

namespace AnFit.Api.DTOs.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public IEnumerable<string>? Errors { get; set; }

        // Constructor cho trường hợp thành công
        public ApiResponse(T data, string message = "Thành công")
        {
            Success = true;
            Message = message;
            Data = data;
        }

        // Constructor cho trường hợp thất bại
        public ApiResponse(string message, IEnumerable<string>? errors = null)
        {
            Success = false;
            Message = message;
            Errors = errors;
        }
    }
}