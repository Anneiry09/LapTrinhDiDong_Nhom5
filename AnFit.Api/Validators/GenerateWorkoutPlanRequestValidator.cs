using AnFit.Api.DTOs.Requests;
using FluentValidation;
using System;

namespace AnFit.Api.Validators
{
    public class GenerateWorkoutPlanRequestValidator : AbstractValidator<GenerateWorkoutPlanRequest>
    {
        public GenerateWorkoutPlanRequestValidator()
        {
            RuleFor(x => x.PlanDate)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Ngày tạo lịch tập không được nằm trong quá khứ.");

            RuleFor(x => x.DifficultyLevel)
                .InclusiveBetween(1, 5)
                .WithMessage("Mức độ khó phải từ 1 (Rất dễ) đến 5 (Rất khó).");
        }
    }
}