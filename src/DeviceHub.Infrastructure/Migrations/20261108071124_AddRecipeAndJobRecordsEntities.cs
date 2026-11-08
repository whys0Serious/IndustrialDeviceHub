using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeAndJobRecordsEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    RecipeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetTemp = table.Column<double>(type: "float", nullable: false),
                    TargetPress = table.Column<double>(type: "float", nullable: false),
                    TargetSpeed = table.Column<double>(type: "float", nullable: false),
                    MaxTemp = table.Column<double>(type: "float", nullable: false),
                    MinTemp = table.Column<double>(type: "float", nullable: false),
                    AvgTemp = table.Column<double>(type: "float", nullable: false),
                    MaxPress = table.Column<double>(type: "float", nullable: false),
                    AvgPress = table.Column<double>(type: "float", nullable: false),
                    MaxSpeed = table.Column<double>(type: "float", nullable: false),
                    AvgSpeed = table.Column<double>(type: "float", nullable: false),
                    IsQualified = table.Column<bool>(type: "bit", nullable: false),
                    ExceptionReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetTemperature = table.Column<double>(type: "float", nullable: false),
                    TargetPressure = table.Column<double>(type: "float", nullable: false),
                    TargetSpeed = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobRecords");

            migrationBuilder.DropTable(
                name: "Recipes");
        }
    }
}
