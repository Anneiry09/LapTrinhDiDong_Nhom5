using System;
using System.Collections.Generic;

namespace AnFit.Api.Models.Entities;

public partial class MedicalCondition
{
    public int Id { get; set; }

    public string? ConditionName { get; set; }

    public int? Type { get; set; }

    public virtual ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
