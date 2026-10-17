namespace AnFit.Api.DTOs.Responses
{
    public class AdminDashboardResponse
    {
        public int TotalUsers { get; set; }
        public int ActiveUsersToday { get; set; }
        public int TotalMealPlans { get; set; }
        public int TotalWorkoutPlans { get; set; }
        public int TotalExercises { get; set; }
        public int TotalFoodItems { get; set; }
    }
}
