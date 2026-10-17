namespace AnFit.Api.Models.Entities;

public class RecipeIngredient
{
    public int Id { get; set; }

    public int RecipeId { get; set; }

    public int FoodItemId { get; set; }

    public double? QuantityGrams { get; set; }

    // 1=Chính (cơm, thịt), 2=Phụ (rau, nấm), 3=Gia vị
    public int? Role { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;

    public virtual FoodItem FoodItem { get; set; } = null!;
}
