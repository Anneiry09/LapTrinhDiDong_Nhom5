using System.Collections.Generic;

namespace AnFit.Api.Models.Entities;

public partial class ActivityCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
