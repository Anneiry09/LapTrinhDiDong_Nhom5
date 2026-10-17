namespace AnFit.Api.DTOs.Requests
{
    public class ExerciseFilterQuery : PaginationQuery
    {
        public string? MuscleGroup { get; set; }
        public string? SearchTerm { get; set; }
    }
}