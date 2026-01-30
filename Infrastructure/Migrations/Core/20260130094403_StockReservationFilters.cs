using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.Core
{
    /// <inheritdoc />
    public partial class StockReservationFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_Cart_Status",
                schema: "core",
                table: "StockReservations",
                columns: new[] { "CartId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_Status_ExpiresAt",
                schema: "core",
                table: "StockReservations",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "UX_StockReservations_Active_Cart_Product",
                schema: "core",
                table: "StockReservations",
                columns: new[] { "CartId", "ProductId" },
                unique: true,
                filter: "[Status] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockReservations_Cart_Status",
                schema: "core",
                table: "StockReservations");

            migrationBuilder.DropIndex(
                name: "IX_StockReservations_Status_ExpiresAt",
                schema: "core",
                table: "StockReservations");

            migrationBuilder.DropIndex(
                name: "UX_StockReservations_Active_Cart_Product",
                schema: "core",
                table: "StockReservations");
        }
    }
}
