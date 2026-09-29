using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlarmResolvedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "Alarms",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "Alarms");
        }
    }
}
