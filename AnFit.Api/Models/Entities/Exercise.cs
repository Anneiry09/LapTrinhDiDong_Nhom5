using System;
using System.Collections.Generic;

namespace AnFit.Api.Models.Entities;

public partial class Exercise
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? MuscleGroup { get; set; }

    public double? BaseCaloriesBurned { get; set; }

    public string? InstructionUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<WorkoutActivity> WorkoutActivities { get; set; } = new List<WorkoutActivity>();
}
