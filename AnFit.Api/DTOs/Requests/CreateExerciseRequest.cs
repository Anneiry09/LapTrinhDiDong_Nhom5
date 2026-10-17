namespace AnFit.Api.DTOs.Requests
{
    public class CreateExerciseRequest
    {
        public string Name { get; set; } = string.Empty;
        public string MuscleGroup { get; set; } = string.Empty;
        public double BaseCaloriesBurned { get; set; }
        public string? InstructionUrl { get; set; }
    }
}