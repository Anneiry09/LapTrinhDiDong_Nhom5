using AnFit.Api.DTOs.Requests;
using FluentValidation;

namespace AnFit.Api.Validators
{
    public class CreateFoodItemRequestValidator : AbstractValidator<CreateFoodItemRequest>
    {
        public CreateFoodItemRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên món ăn không được để trống.")
                .MaximumLength(255).WithMessage("Tên món ăn không được vượt quá 255 ký tự.");

            RuleFor(x => x.CaloriesPerUnit)
                .GreaterThan(0).WithMessage("Lượng calo phải lớn hơn 0.");

            RuleFor(x => x.EstimatedPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Giá tiền ước tính không được âm.");

            RuleFor(x => x.Unit)
                .NotEmpty().WithMessage("Đơn vị tính không được để trống.");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Nhóm thực phẩm không được để trống.")
                .InclusiveBetween(1, 7).WithMessage("Nhóm thực phẩm phải từ 1 đến 7 (1=Carbs, 2=Protein, 3=Rau, 4=Trái cây, 5=Chất béo/Hạt, 6=Đồ uống, 7=Gia vị).");
        }
    }
}