using System.Collections.Generic;

namespace AnFit.Api.DTOs.Requests
{
    public class UpdateMedicalConditionsRequest
    {
        public IEnumerable<int> ConditionIds { get; set; } = new List<int>();
    }
}