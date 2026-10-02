using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceModbusFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableMonitoring",
                table: "Devices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ModbusRegisterCount",
                table: "Devices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "ModbusSlaveId",
                table: "Devices",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModbusStartAddress",
                table: "Devices",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableMonitoring",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "ModbusRegisterCount",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "ModbusSlaveId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "ModbusStartAddress",
                table: "Devices");
        }
    }
}
