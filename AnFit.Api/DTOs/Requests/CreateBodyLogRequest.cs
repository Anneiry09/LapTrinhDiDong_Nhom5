using System;

namespace AnFit.Api.DTOs.Requests
{
    public class CreateBodyLogRequest
    {
        public double Weight { get; set; }
        public double? BodyFatPercentage { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}