using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.Core
{
    /// <inheritdoc />
    public partial class Shipping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServicePointAddress",
                schema: "core",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServicePointId",
                schema: "core",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServicePointName",
                schema: "core",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShippingCarrier",
                schema: "core",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShippingMethod",
                schema: "core",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServicePointAddress",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ServicePointId",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ServicePointName",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingCarrier",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingMethod",
                schema: "core",
                table: "Orders");
        }
    }
}
