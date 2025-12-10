using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.Core
{
    /// <inheritdoc />
    public partial class OrderRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Total",
                schema: "core",
                table: "Orders",
                newName: "TaxTotal");

            migrationBuilder.RenameColumn(
                name: "ProductsTotal",
                schema: "core",
                table: "Orders",
                newName: "ProductsSubtotal");

            migrationBuilder.RenameColumn(
                name: "OrderDate",
                schema: "core",
                table: "Orders",
                newName: "UpdatedAt");

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                schema: "core",
                table: "Products",
                type: "decimal(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "OrderStatus",
                schema: "core",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "core",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "core",
                table: "Orders",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "GrandTotal",
                schema: "core",
                table: "Orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Payment_AmountAuthorized",
                schema: "core",
                table: "Orders",
                type: "nvarchar(max)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Payment_AmountCaptured",
                schema: "core",
                table: "Orders",
                type: "nvarchar(max)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Payment_AmountRefunded",
                schema: "core",
                table: "Orders",
                type: "nvarchar(max)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Payment_AuthorizedAt",
                schema: "core",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Payment_CapturedAt",
                schema: "core",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Payment_Currency",
                schema: "core",
                table: "Orders",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Payment_LatestChargeId",
                schema: "core",
                table: "Orders",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Payment_PaymentIntentId",
                schema: "core",
                table: "Orders",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Payment_PaymentMethodType",
                schema: "core",
                table: "Orders",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Payment_RefundedAt",
                schema: "core",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Payment_Status",
                schema: "core",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                schema: "core",
                table: "OrderItems",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                schema: "core",
                table: "OrderItems",
                type: "decimal(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                schema: "core",
                table: "Orders",
                column: "OrderNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderNumber",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "VatRate",
                schema: "core",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GrandTotal",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Payment_AmountAuthorized",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Payment_AmountCaptured",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Payment_AmountRefunded",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Payment_AuthorizedAt",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Payment_CapturedAt",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Payment_Currency",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Payment_LatestChargeId",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Payment_PaymentIntentId",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Payment_PaymentMethodType",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Payment_RefundedAt",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Payment_Status",
                schema: "core",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Sku",
                schema: "core",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "VatRate",
                schema: "core",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "core",
                table: "Orders",
                newName: "OrderDate");

            migrationBuilder.RenameColumn(
                name: "TaxTotal",
                schema: "core",
                table: "Orders",
                newName: "Total");

            migrationBuilder.RenameColumn(
                name: "ProductsSubtotal",
                schema: "core",
                table: "Orders",
                newName: "ProductsTotal");

            migrationBuilder.AlterColumn<string>(
                name: "OrderStatus",
                schema: "core",
                table: "Orders",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
