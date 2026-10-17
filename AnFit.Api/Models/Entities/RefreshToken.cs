using System;
using System.Collections.Generic;

namespace AnFit.Api.Models.Entities;

public partial class RefreshToken
{
    public int Id { get; set; }

    public Guid? UserId { get; set; }

    public string? Token { get; set; }

    public string? DeviceName { get; set; }

    public string? IpAddress { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public string? ReplacedByToken { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
