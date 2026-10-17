using System;
using System.Collections.Generic;

namespace AnFit.Api.Models.Entities;

public partial class FoodItem
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public double? CaloriesPerUnit { get; set; }

    public double? ProteinPerUnit { get; set; }

    public double? CarbsPerUnit { get; set; }

    public double? FatPerUnit { get; set; }

    public decimal? EstimatedPrice { get; set; }

    public string? Unit { get; set; }

    // Nhóm thực phẩm: 1=Carbs, 2=Protein, 3=Rau, 4=Trái cây, 5=Chất béo/Hạt, 6=Đồ uống, 7=Gia vị
    public int? Category { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<MealPlanDetail> MealPlanDetails { get; set; } = new List<MealPlanDetail>();

    public virtual ICollection<MedicalCondition> Conditions { get; set; } = new List<MedicalCondition>();

    public virtual ICollection<FoodPriceHistory> PriceHistory { get; set; } = new List<FoodPriceHistory>();
}
