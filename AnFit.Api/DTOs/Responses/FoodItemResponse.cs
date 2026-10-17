using System.Collections.Generic;

namespace AnFit.Api.DTOs.Responses
{
    public class FoodItemResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double CaloriesPerUnit { get; set; }
        public double ProteinPerUnit { get; set; }
        public double CarbsPerUnit { get; set; }
        public double FatPerUnit { get; set; }
        public decimal EstimatedPrice { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int Category { get; set; } // 1=Carbs, 2=Protein, 3=Rau, 4=Trái cây, 5=Béo/Hạt, 6=Đồ uống, 7=Gia vị

        // Trả về tên các loại bệnh lý cảnh báo (thay vì chỉ trả ID) để Frontend dễ hiển thị tag
        public IEnumerable<string>? HealthWarnings { get; set; }
    }
}