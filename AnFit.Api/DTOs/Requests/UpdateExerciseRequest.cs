namespace AnFit.Api.DTOs.Requests
{
    public class UpdateExerciseRequest
    {
        public string? Name { get; set; }
        public string? MuscleGroup { get; set; }
        public double? BaseCaloriesBurned { get; set; }
        public string? InstructionUrl { get; set; }
    }
}