using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.Core
{
    /// <inheritdoc />
    public partial class OrderEnityFulfillment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                schema: "core",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FulfilledAt",
                schema: "core",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FulfillmentNote",
                schema: "core",
                table: "Orders",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FulfillmentStatus",
                schema: "core",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_FulfilledAt",
                schema: "core",
                table: "Orders",
                column: "FulfilledAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_FulfillmentStatus",
                schema: "core",
                table: "Orders",
                column: "FulfillmentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_FulfillmentStatus_ConfirmedAt",
                schema: "core",
                table: "Orders",
                columns: new[] { "FulfillmentStatus", "ConfirmedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderStatus",
                schema: "core",
                table: "Orders",
                column: "OrderStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_FulfilledAt",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_FulfillmentStatus",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_FulfillmentStatus_ConfirmedAt",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderStatus",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FulfilledAt",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FulfillmentNote",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FulfillmentStatus",
                schema: "core",
                table: "Orders");
        }
    }
}
