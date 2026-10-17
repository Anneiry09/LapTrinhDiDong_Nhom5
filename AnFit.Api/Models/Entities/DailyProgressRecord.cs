using System;

namespace AnFit.Api.Models.Entities;

public partial class DailyProgressRecord
{
    public int Id { get; set; }

    public Guid? UserId { get; set; }

    public DateOnly? RecordDate { get; set; }

    // Dinh dưỡng
    public double? TotalCaloriesConsumed { get; set; }
    public double? TotalProtein { get; set; }
    public double? TotalCarbs { get; set; }
    public double? TotalFat { get; set; }

    // Tập luyện & Vận động
    public string? WorkoutPlanName { get; set; }
    public double? TotalCaloriesBurned { get; set; }
    public int? TotalSteps { get; set; }

    // Chỉ số cơ thể lúc đó
    public double? CurrentWeight { get; set; }

    // Audit fields
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public virtual User? User { get; set; }
}
