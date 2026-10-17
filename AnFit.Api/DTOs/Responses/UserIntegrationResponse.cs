using System;

namespace AnFit.Api.DTOs.Responses
{
    public class UserIntegrationResponse
    {
        public int Provider { get; set; }
        public string? ExternalAccountId { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public bool IsConnected => !string.IsNullOrEmpty(ExternalAccountId);
    }
}