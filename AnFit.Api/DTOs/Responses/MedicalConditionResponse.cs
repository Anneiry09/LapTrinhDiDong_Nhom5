namespace AnFit.Api.DTOs.Responses
{
    public class MedicalConditionResponse
    {
        public int Id { get; set; }
        public string ConditionName { get; set; } = string.Empty;
        public int Type { get; set; }
    }
}