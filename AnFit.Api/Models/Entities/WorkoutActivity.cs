using System;
using System.Collections.Generic;

namespace AnFit.Api.Models.Entities;

public partial class WorkoutActivity
{
    public int Id { get; set; }

    public int? WorkoutPlanId { get; set; }

    public int? ExerciseId { get; set; }

    public int? SetsCount { get; set; }

    public int? RepsPerSet { get; set; }

    public int? RestTimeSeconds { get; set; }

    public virtual Exercise? Exercise { get; set; }

    public virtual WorkoutPlan? WorkoutPlan { get; set; }
}
