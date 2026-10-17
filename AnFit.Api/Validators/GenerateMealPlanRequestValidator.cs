using AnFit.Api.DTOs.Requests;
using FluentValidation;
using System;

namespace AnFit.Api.Validators
{
    public class GenerateMealPlanRequestValidator : AbstractValidator<GenerateMealPlanRequest>
    {
        public GenerateMealPlanRequestValidator()
        {
            RuleFor(x => x.PlanDate)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Ngày tạo thực đơn không được nằm trong quá khứ.");

            RuleFor(x => x.TargetCalories)
                .InclusiveBetween(500, 10000).When(x => x.TargetCalories.HasValue)
                .WithMessage("Lượng calo mục tiêu (nếu có) phải nằm trong khoảng từ 500 đến 10000 kcal.");

            RuleFor(x => x.BudgetLimit)
                .GreaterThanOrEqualTo(0).When(x => x.BudgetLimit.HasValue)
                .WithMessage("Ngân sách tối đa không được là số âm.");
        }
    }
}