namespace AnFit.Api.DTOs.Responses
{
    public class MealPlanDetailResponse
    {
        public int Id { get; set; }
        public int MealType { get; set; }
        public double ServingSize { get; set; }      // Quantity (gram)
        public double CalculatedCalories { get; set; }
        public double CalculatedProtein { get; set; }
        public double CalculatedCarbs { get; set; }
        public double CalculatedFat { get; set; }
        public decimal CalculatedPrice { get; set; }
        public bool IsTracked { get; set; }
        public int? RecipeId { get; set; }
        public string? RecipeName { get; set; }
        public FoodItemResponse? FoodItem { get; set; } // Lồng DTO — fallback cho data cũ
    }
}