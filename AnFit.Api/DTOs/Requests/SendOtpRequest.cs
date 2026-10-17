using System.ComponentModel.DataAnnotations;

namespace AnFit.Api.DTOs.Requests
{
    public class SendOtpRequest
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;
    }
}
