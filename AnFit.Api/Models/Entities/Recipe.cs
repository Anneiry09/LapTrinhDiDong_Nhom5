using System;
using System.Collections.Generic;

namespace AnFit.Api.Models.Entities;

public class Recipe
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public double? TotalCalories { get; set; }

    public double? TotalProtein { get; set; }

    public double? TotalCarbs { get; set; }

    public double? TotalFat { get; set; }

    public decimal? EstimatedPrice { get; set; }

    // Bitmask: 1=Breakfast, 2=Lunch, 4=Dinner, 8=Snack (VD: 6 = Lunch+Dinner)
    public int? MealSuitability { get; set; }

    // 1=Món cơm, 2=Món bún/phở/mì, 3=Món rau/salad, 4=Snack/bánh, 5=Đồ uống/sinh tố
    public int? Category { get; set; }

    public int? CookingTimeMinutes { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();

    public virtual ICollection<MealPlanDetail> MealPlanDetails { get; set; } = new List<MealPlanDetail>();
}
