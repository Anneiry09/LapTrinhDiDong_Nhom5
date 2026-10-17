using AnFit.Api.DTOs.Requests;
using FluentValidation;

namespace AnFit.Api.Validators
{
    public class RegisterWithOnboardingRequestValidator : AbstractValidator<RegisterWithOnboardingRequest>
    {
        public RegisterWithOnboardingRequestValidator()
        {
            // --- Auth ---
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Email không đúng định dạng.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.")
                .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Mật khẩu xác nhận không khớp.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ tên không được để trống.")
                .MaximumLength(255);

            // --- Bio ---
            RuleFor(x => x.Gender)
                .InclusiveBetween(0, 2).When(x => x.Gender.HasValue)
                .WithMessage("Giới tính không hợp lệ.");

            RuleFor(x => x.HeightCm)
                .InclusiveBetween(50, 300).When(x => x.HeightCm.HasValue)
                .WithMessage("Chiều cao phải từ 50 đến 300 cm.");

            RuleFor(x => x.Weight)
                .GreaterThan(0).When(x => x.Weight.HasValue)
                .WithMessage("Cân nặng phải lớn hơn 0.");

            // --- Goal ---
            RuleFor(x => x.TargetGoal)
                .InclusiveBetween(1, 3).When(x => x.TargetGoal.HasValue)
                .WithMessage("Mục tiêu không hợp lệ.");

            RuleFor(x => x.DailyBudget)
                .GreaterThanOrEqualTo(0).When(x => x.DailyBudget.HasValue)
                .WithMessage("Ngân sách không hợp lệ.");

            // --- Onboarding ---
            RuleFor(x => x.Occupation)
                .InclusiveBetween(1, 5).When(x => x.Occupation.HasValue)
                .WithMessage("Nghề nghiệp không hợp lệ.");

            RuleFor(x => x.CurrentBodyShape)
                .InclusiveBetween(1, 10).When(x => x.CurrentBodyShape.HasValue)
                .WithMessage("Cấp độ vóc dáng không hợp lệ.");

            RuleFor(x => x.TargetBodyShape)
                .InclusiveBetween(1, 10).When(x => x.TargetBodyShape.HasValue)
                .WithMessage("Cấp độ vóc dáng mục tiêu không hợp lệ.");

            RuleFor(x => x.FitnessLevel)
                .InclusiveBetween(1, 3).When(x => x.FitnessLevel.HasValue)
                .WithMessage("Trình độ thể chất không hợp lệ.");

            RuleFor(x => x.PreferredIntensity)
                .InclusiveBetween(1, 3).When(x => x.PreferredIntensity.HasValue)
                .WithMessage("Cường độ mong muốn không hợp lệ.");

            RuleFor(x => x.PsychologicalBarrier)
                .InclusiveBetween(1, 5).When(x => x.PsychologicalBarrier.HasValue)
                .WithMessage("Rào cản tâm lý không hợp lệ.");
        }
    }
}
