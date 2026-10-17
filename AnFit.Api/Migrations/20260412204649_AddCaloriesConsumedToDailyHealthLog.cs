using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnFit.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCaloriesConsumedToDailyHealthLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CaloriesConsumed",
                table: "DailyHealthLogs",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaloriesConsumed",
                table: "DailyHealthLogs");
        }
    }
}
