using AnFit.Api.DTOs.Requests;
using FluentValidation;

namespace AnFit.Api.Validators
{
    public class CreateExerciseRequestValidator : AbstractValidator<CreateExerciseRequest>
    {
        public CreateExerciseRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên bài tập không được để trống.")
                .MaximumLength(255).WithMessage("Tên bài tập không được vượt quá 255 ký tự.");

            RuleFor(x => x.BaseCaloriesBurned)
                .GreaterThan(0).WithMessage("Lượng calo đốt cháy phải lớn hơn 0.");

            RuleFor(x => x.InstructionUrl)
                .Matches(@"^(https?://)?([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$")
                .When(x => !string.IsNullOrEmpty(x.InstructionUrl))
                .WithMessage("Đường dẫn URL không hợp lệ.");
        }
    }
}