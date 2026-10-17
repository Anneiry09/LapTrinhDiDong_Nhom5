using AnFit.Api.DTOs.Requests;
using FluentValidation;

namespace AnFit.Api.Validators
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Token).NotEmpty().WithMessage("Token không hợp lệ.");
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).WithMessage("Mật khẩu mới phải có ít nhất 8 ký tự.");
            RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword).WithMessage("Mật khẩu xác nhận không khớp.");
        }
    }
}