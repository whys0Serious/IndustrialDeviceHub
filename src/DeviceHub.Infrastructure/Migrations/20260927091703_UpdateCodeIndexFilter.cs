using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCodeIndexFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Devices_Code",
                table: "Devices");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_Code",
                table: "Devices",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Devices_Code",
                table: "Devices");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_Code",
                table: "Devices",
                column: "Code",
                unique: true);
        }
    }
}
