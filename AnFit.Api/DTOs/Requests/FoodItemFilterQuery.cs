namespace AnFit.Api.DTOs.Requests
{
    public class FoodItemFilterQuery : PaginationQuery
    {
        public string? SearchTerm { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? Category { get; set; }
    }
}