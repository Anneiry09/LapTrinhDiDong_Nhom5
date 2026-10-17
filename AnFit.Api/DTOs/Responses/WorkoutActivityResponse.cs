namespace AnFit.Api.DTOs.Responses
{
    public class WorkoutActivityResponse
    {
        public int Id { get; set; }
        public int Sets { get; set; }
        public int RepsPerSet { get; set; }
        public int RestTimeSeconds { get; set; }
        public double? DurationMinutes { get; set; }
        public double EstimatedCaloriesBurned { get; set; }
        public string? Phase { get; set; }
        public ExerciseResponse? Exercise { get; set; } // Lồng DTO
    }
}