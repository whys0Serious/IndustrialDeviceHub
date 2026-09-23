using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRemarkAndDeletedAtToDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Devices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remark",
                table: "Devices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Remark",
                table: "Devices");
        }
    }
}
