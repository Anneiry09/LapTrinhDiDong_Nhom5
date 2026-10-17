using System.Collections.Generic;

namespace AnFit.Api.DTOs.Requests
{
    public class CreateFoodItemRequest
    {
        public string Name { get; set; } = string.Empty;
        public double CaloriesPerUnit { get; set; }
        public double ProteinPerUnit { get; set; }
        public double CarbsPerUnit { get; set; }
        public double FatPerUnit { get; set; }
        public decimal EstimatedPrice { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int Category { get; set; }

        // Danh sách ID các bệnh lý bị cảnh báo/dị ứng với món ăn này
        public IEnumerable<int>? ConditionIds { get; set; }
    }
}