using System;
using System.Collections.Generic;

namespace AnFit.Api.Models.Entities;

public partial class MealPlanDetail
{
    public int Id { get; set; }

    public int? MealPlanId { get; set; }

    public int? FoodItemId { get; set; }

    public int? RecipeId { get; set; }

    public int? MealType { get; set; }

    public double? Quantity { get; set; }

    public double? CalculatedCalories { get; set; }

    public double? CalculatedProtein { get; set; }

    public double? CalculatedCarbs { get; set; }

    public double? CalculatedFat { get; set; }

    public decimal? CalculatedPrice { get; set; }

    // User đã xác nhận ăn xong chưa (Track feature)
    public bool IsTracked { get; set; }

    public virtual FoodItem? FoodItem { get; set; }

    public virtual Recipe? Recipe { get; set; }

    public virtual MealPlan? MealPlan { get; set; }
}
