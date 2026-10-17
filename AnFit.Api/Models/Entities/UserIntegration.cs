using System;
using System.Collections.Generic;

namespace AnFit.Api.Models.Entities;

public partial class UserIntegration
{
    public int Id { get; set; }

    public Guid? UserId { get; set; }

    public int? Provider { get; set; }

    public string? ExternalAccountId { get; set; }

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? TokenExpiry { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? User { get; set; }
}
