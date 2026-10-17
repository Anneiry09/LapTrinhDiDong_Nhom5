using System;
using System.Collections.Generic;

namespace AnFit.Api.Models.Entities;

public partial class WorkoutPlan
{
    public int Id { get; set; }

    public Guid? UserId { get; set; }

    public DateOnly? PlanDate { get; set; }

    public double? EstimatedCaloriesBurn { get; set; }

    public int? Status { get; set; }

    public int? DifficultyRating { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<WorkoutActivity> WorkoutActivities { get; set; } = new List<WorkoutActivity>();
}
