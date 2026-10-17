using System;
using System.Collections.Generic;

namespace AnFit.Api.Models.Entities;

public partial class DailyHealthLog
{
    public int Id { get; set; }

    public Guid? UserId { get; set; }

    public DateOnly? RecordDate { get; set; }

    public string? ExternalSourceId { get; set; }

    public int? TotalSteps { get; set; }

    public double? ActiveCalories { get; set; }

    public double? CaloriesConsumed { get; set; }

    public int? RestingHeartRate { get; set; }

    public int? MaxHeartRate { get; set; }

    public int? SleepDurationMinutes { get; set; }

    public int? StressLevel { get; set; }

    public double? CalculatedTdee { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? User { get; set; }
}
