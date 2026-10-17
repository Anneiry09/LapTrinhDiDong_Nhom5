using AnFit.Api.DTOs.Requests;
using FluentValidation;
using System;

namespace AnFit.Api.Validators
{
    public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
    {
        public UpdateProfileRequestValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.HeightCm).InclusiveBetween(50, 300).WithMessage("Chiều cao phải từ 50cm đến 300cm.");
            RuleFor(x => x.DateOfBirth).LessThan(DateTime.UtcNow).When(x => x.DateOfBirth.HasValue);
            RuleFor(x => x.DailyBudget).GreaterThanOrEqualTo(0);
            // Các enum Gender, TargetGoal có thể kiểm tra IsInEnum() nếu bạn muốn chặt chẽ hơn
        }
    }
}