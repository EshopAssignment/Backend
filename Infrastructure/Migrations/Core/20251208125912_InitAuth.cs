using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.Core
{
    /// <inheritdoc />
    public partial class InitAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.RenameTable(
                name: "StockReservations",
                newName: "StockReservations",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Products",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "Orders",
                newName: "Orders",
                newSchema: "core");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                newName: "OrderItems",
                newSchema: "core");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "StockReservations",
                schema: "core",
                newName: "StockReservations");

            migrationBuilder.RenameTable(
                name: "Products",
                schema: "core",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "Orders",
                schema: "core",
                newName: "Orders");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                schema: "core",
                newName: "OrderItems");
        }
    }
}
