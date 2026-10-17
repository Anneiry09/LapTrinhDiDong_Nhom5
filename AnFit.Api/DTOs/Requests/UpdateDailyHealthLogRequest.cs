namespace AnFit.Api.DTOs.Requests
{
    public class UpdateDailyHealthLogRequest
    {
        public double? CaloriesConsumed { get; set; }
        public double? ActiveCalories { get; set; }
        public int? StepCount { get; set; }
        public int? SleepDurationMinutes { get; set; }
        public int? RestingHeartRate { get; set; }
    }
}