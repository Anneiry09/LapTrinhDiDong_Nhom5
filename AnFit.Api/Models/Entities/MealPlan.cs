using System;
using System.Collections.Generic;

namespace AnFit.Api.Models.Entities;

public partial class MealPlan
{
    public int Id { get; set; }

    public Guid? UserId { get; set; }

    public DateOnly? PlanDate { get; set; }

    public double? TotalCalories { get; set; }

    public double? TotalProtein { get; set; }

    public double? TotalCarbs { get; set; }

    public double? TotalFat { get; set; }

    public decimal? TotalPrice { get; set; }

    public int? Status { get; set; }

    public int? UserRating { get; set; }

    /// <summary>Cảnh báo ngân sách từ AI (VD: "Ngân sách thấp, ưu tiên protein rẻ")</summary>
    public string? BudgetWarning { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<MealPlanDetail> MealPlanDetails { get; set; } = new List<MealPlanDetail>();

    public virtual User? User { get; set; }
}
