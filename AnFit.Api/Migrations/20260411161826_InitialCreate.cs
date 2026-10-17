using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnFit.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MuscleGroup = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BaseCaloriesBurned = table.Column<double>(type: "float", nullable: true),
                    InstructionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Exercise__3214EC0702782631", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FoodItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CaloriesPerUnit = table.Column<double>(type: "float", nullable: true),
                    ProteinPerUnit = table.Column<double>(type: "float", nullable: true),
                    CarbsPerUnit = table.Column<double>(type: "float", nullable: true),
                    FatPerUnit = table.Column<double>(type: "float", nullable: true),
                    EstimatedPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FoodItem__3214EC0791A384E4", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedicalConditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConditionName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MedicalC__3214EC07F15B4426", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: true, defaultValue: 1),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    PasswordResetToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ResetTokenExpiry = table.Column<DateTime>(type: "datetime", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    HeightCm = table.Column<double>(type: "float", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime", nullable: true),
                    TargetGoal = table.Column<int>(type: "int", nullable: true),
                    DailyBudget = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__3214EC07AB13AA58", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FoodHealthWarnings",
                columns: table => new
                {
                    FoodItemId = table.Column<int>(type: "int", nullable: false),
                    ConditionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FoodHeal__A532941EAE83FE7B", x => new { x.FoodItemId, x.ConditionId });
                    table.ForeignKey(
                        name: "FK__FoodHealt__Condi__5CD6CB2B",
                        column: x => x.ConditionId,
                        principalTable: "MedicalConditions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__FoodHealt__FoodI__5BE2A6F2",
                        column: x => x.FoodItemId,
                        principalTable: "FoodItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DailyHealthLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecordDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExternalSourceId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TotalSteps = table.Column<int>(type: "int", nullable: true),
                    ActiveCalories = table.Column<double>(type: "float", nullable: true),
                    RestingHeartRate = table.Column<int>(type: "int", nullable: true),
                    MaxHeartRate = table.Column<int>(type: "int", nullable: true),
                    SleepDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    StressLevel = table.Column<int>(type: "int", nullable: true),
                    CalculatedTDEE = table.Column<double>(type: "float", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DailyHea__3214EC076A5B1858", x => x.Id);
                    table.ForeignKey(
                        name: "FK__DailyHeal__UserI__5AEE82B9",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MealPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PlanDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TotalCalories = table.Column<double>(type: "float", nullable: true),
                    TotalProtein = table.Column<double>(type: "float", nullable: true),
                    TotalCarbs = table.Column<double>(type: "float", nullable: true),
                    TotalFat = table.Column<double>(type: "float", nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    UserRating = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MealPlan__3214EC07D50D9151", x => x.Id);
                    table.ForeignKey(
                        name: "FK__MealPlans__UserI__5DCAEF64",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeviceName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    ReplacedByToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RefreshT__3214EC0799C46DF8", x => x.Id);
                    table.ForeignKey(
                        name: "FK__RefreshTo__UserI__5629CD9C",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserBodyLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    Weight = table.Column<double>(type: "float", nullable: true),
                    BodyFatPercentage = table.Column<double>(type: "float", nullable: true),
                    BMI = table.Column<double>(type: "float", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserBody__3214EC078F4B08DA", x => x.Id);
                    table.ForeignKey(
                        name: "FK__UserBodyL__UserI__571DF1D5",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserIntegrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Provider = table.Column<int>(type: "int", nullable: true),
                    ExternalAccountId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AccessToken = table.Column<string>(type: "text", nullable: true),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    TokenExpiry = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserInte__3214EC077CFFEC7E", x => x.Id);
                    table.ForeignKey(
                        name: "FK__UserInteg__UserI__59FA5E80",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserMedicalConditions",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConditionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserMedi__F4F790408B9798AE", x => new { x.UserId, x.ConditionId });
                    table.ForeignKey(
                        name: "FK__UserMedic__Condi__59063A47",
                        column: x => x.ConditionId,
                        principalTable: "MedicalConditions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__UserMedic__UserI__5812160E",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkoutPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PlanDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EstimatedCaloriesBurn = table.Column<double>(type: "float", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    DifficultyRating = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WorkoutP__3214EC07ED540981", x => x.Id);
                    table.ForeignKey(
                        name: "FK__WorkoutPl__UserI__60A75C0F",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MealPlanDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MealPlanId = table.Column<int>(type: "int", nullable: true),
                    FoodItemId = table.Column<int>(type: "int", nullable: true),
                    MealType = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<double>(type: "float", nullable: true),
                    CalculatedCalories = table.Column<double>(type: "float", nullable: true),
                    CalculatedPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MealPlan__3214EC07530A096B", x => x.Id);
                    table.ForeignKey(
                        name: "FK__MealPlanD__FoodI__5FB337D6",
                        column: x => x.FoodItemId,
                        principalTable: "FoodItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__MealPlanD__MealP__5EBF139D",
                        column: x => x.MealPlanId,
                        principalTable: "MealPlans",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkoutActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkoutPlanId = table.Column<int>(type: "int", nullable: true),
                    ExerciseId = table.Column<int>(type: "int", nullable: true),
                    SetsCount = table.Column<int>(type: "int", nullable: true),
                    RepsPerSet = table.Column<int>(type: "int", nullable: true),
                    RestTimeSeconds = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WorkoutA__3214EC07FF6195DD", x => x.Id);
                    table.ForeignKey(
                        name: "FK__WorkoutAc__Exerc__628FA481",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK__WorkoutAc__Worko__619B8048",
                        column: x => x.WorkoutPlanId,
                        principalTable: "WorkoutPlans",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "DailyHealthLogs_index_1",
                table: "DailyHealthLogs",
                columns: new[] { "UserId", "RecordDate" },
                unique: true,
                filter: "[UserId] IS NOT NULL AND [RecordDate] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "DailyHealthLogs_index_2",
                table: "DailyHealthLogs",
                columns: new[] { "UserId", "ExternalSourceId" },
                unique: true,
                filter: "[UserId] IS NOT NULL AND [ExternalSourceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FoodHealthWarnings_ConditionId",
                table: "FoodHealthWarnings",
                column: "ConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_MealPlanDetails_FoodItemId",
                table: "MealPlanDetails",
                column: "FoodItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MealPlanDetails_MealPlanId",
                table: "MealPlanDetails",
                column: "MealPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_MealPlans_UserId",
                table: "MealPlans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ__RefreshT__1EB4F817438826CA",
                table: "RefreshTokens",
                column: "Token",
                unique: true,
                filter: "[Token] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserBodyLogs_UserId",
                table: "UserBodyLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UserIntegrations_index_0",
                table: "UserIntegrations",
                columns: new[] { "UserId", "Provider" },
                unique: true,
                filter: "[UserId] IS NOT NULL AND [Provider] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserMedicalConditions_ConditionId",
                table: "UserMedicalConditions",
                column: "ConditionId");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D10534CFAAD8AB",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutActivities_ExerciseId",
                table: "WorkoutActivities",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutActivities_WorkoutPlanId",
                table: "WorkoutActivities",
                column: "WorkoutPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlans_UserId",
                table: "WorkoutPlans",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyHealthLogs");

            migrationBuilder.DropTable(
                name: "FoodHealthWarnings");

            migrationBuilder.DropTable(
                name: "MealPlanDetails");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "UserBodyLogs");

            migrationBuilder.DropTable(
                name: "UserIntegrations");

            migrationBuilder.DropTable(
                name: "UserMedicalConditions");

            migrationBuilder.DropTable(
                name: "WorkoutActivities");

            migrationBuilder.DropTable(
                name: "FoodItems");

            migrationBuilder.DropTable(
                name: "MealPlans");

            migrationBuilder.DropTable(
                name: "MedicalConditions");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "WorkoutPlans");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
