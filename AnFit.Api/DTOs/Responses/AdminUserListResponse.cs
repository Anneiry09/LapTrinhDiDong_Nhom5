using System;

namespace AnFit.Api.DTOs.Responses
{
    public class AdminUserListResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int Role { get; set; }
        public int? Gender { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? TargetGoal { get; set; }
        public double? Weight { get; set; }
    }
}
