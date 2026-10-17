using System;

namespace AnFit.Api.DTOs.Responses
{
    public class BodyLogResponse
    {
        public int Id { get; set; }
        public double Weight { get; set; }
        public double? BodyFatPercentage { get; set; }
        public double? Bmi { get; set; } // Sẽ được hệ thống tự tính toán và trả về
        public DateTime RecordedAt { get; set; }
    }
}