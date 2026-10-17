using System.Collections.Generic;

namespace AnFit.Api.DTOs.Requests
{
    public class UpdateFoodItemRequest
    {
        public string? Name { get; set; }
        public double? CaloriesPerUnit { get; set; }
        public double? ProteinPerUnit { get; set; }
        public double? CarbsPerUnit { get; set; }
        public double? FatPerUnit { get; set; }
        public decimal? EstimatedPrice { get; set; }
        public string? Unit { get; set; }
        public int? Category { get; set; }
        public IEnumerable<int>? ConditionIds { get; set; }
    }
}