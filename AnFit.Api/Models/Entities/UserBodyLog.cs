using System;
using System.Collections.Generic;

namespace AnFit.Api.Models.Entities;

public partial class UserBodyLog
{
    public int Id { get; set; }

    public Guid? UserId { get; set; }

    public DateTime? RecordedAt { get; set; }

    public double? Weight { get; set; }

    public double? BodyFatPercentage { get; set; }

    public double? Bmi { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? User { get; set; }
}
