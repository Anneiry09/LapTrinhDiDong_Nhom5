using System;

namespace AnFit.Api.DTOs.Requests
{
    public class SyncHealthConnectRequest
    {
        public int TotalSteps { get; set; }
        public double ActiveCalories { get; set; }
        public int RestingHeartRate { get; set; }
        public int MaxHeartRate { get; set; }
        public int SleepDurationMinutes { get; set; }
        public double StressLevel { get; set; }
        public DateOnly TargetDate { get; set; }
    }
}
