using System;

namespace AnFit.Api.DTOs.Responses
{
    public class DailyHealthLogResponse
    {
        public int Id { get; set; }
        public DateOnly RecordDate { get; set; }
        public double Tdee { get; set; }
        public double? CaloriesConsumed { get; set; }
        public double? CaloriesBurned { get; set; }
        public double? ActiveCalories { get; set; }
        public int? StepCount { get; set; }
        public int? SleepDurationMinutes { get; set; }
        public int? RestingHeartRate { get; set; }
        public int? MaxHeartRate { get; set; }
    }
}