namespace AnFit.Api.DTOs.Responses
{
    public class ExerciseResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MuscleGroup { get; set; } = string.Empty;
        public double BaseCaloriesBurned { get; set; }
        public string? InstructionUrl { get; set; }
    }
}