using AnFit.Api.DTOs.Requests;
using FluentValidation;
using System;

namespace AnFit.Api.Validators
{
    public class CreateBodyLogRequestValidator : AbstractValidator<CreateBodyLogRequest>
    {
        public CreateBodyLogRequestValidator()
        {
            RuleFor(x => x.Weight)
                .InclusiveBetween(20, 300).WithMessage("Cân nặng không hợp lý (phải từ 20kg đến 300kg).");

            RuleFor(x => x.BodyFatPercentage)
                .InclusiveBetween(1, 70).When(x => x.BodyFatPercentage.HasValue)
                .WithMessage("Tỷ lệ mỡ không hợp lý (phải từ 1% đến 70%).");

            RuleFor(x => x.RecordedAt)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Thời gian ghi nhận không được lớn hơn hiện tại.");
        }
    }
}