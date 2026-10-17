using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AnFit.Api.DTOs.Requests
{
    public class CreateRecipeRequestDto
    {
        [Required(ErrorMessage = "Tên món ăn không được để trống.")]
        public string Name { get; set; } = null!;

        public double? TotalCalories { get; set; }

        public double? TotalProtein { get; set; }

        public double? TotalCarbs { get; set; }

        public double? TotalFat { get; set; }

        public decimal? EstimatedPrice { get; set; }

        public int? MealSuitability { get; set; }

        public int? Category { get; set; }

        public int? CookingTimeMinutes { get; set; }

        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Món ăn bắt buộc phải có ít nhất 1 nguyên liệu.")]
        [MinLength(1, ErrorMessage = "Danh sách nguyên liệu không được để trống.")]
        public List<CreateRecipeIngredientDto> Ingredients { get; set; } = new List<CreateRecipeIngredientDto>();
    }

    public class CreateRecipeIngredientDto
    {
        [Required(ErrorMessage = "Vui lòng chọn nguyên liệu hợp lệ.")]
        public int FoodItemId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số gram.")]
        [Range(0.1, 10000, ErrorMessage = "Khối lượng nguyên liệu phải lớn hơn 0.")]
        public double? QuantityGrams { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò nguyên liệu.")]
        [Range(1, 3, ErrorMessage = "Vai trò chỉ được nhận giá trị từ 1 đến 3 (1: Chính, 2: Phụ, 3: Gia vị).")]
        public int? Role { get; set; }
    }
}
