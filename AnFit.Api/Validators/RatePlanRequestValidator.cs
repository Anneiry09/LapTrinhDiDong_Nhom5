using AnFit.Api.DTOs.Requests;
using FluentValidation;

namespace AnFit.Api.Validators
{
    public class RatePlanRequestValidator : AbstractValidator<RatePlanRequest>
    {
        public RatePlanRequestValidator()
        {
            RuleFor(x => x.Rating).InclusiveBetween(1, 5).WithMessage("Đánh giá phải từ 1 đến 5 sao.");
        }
    }
}