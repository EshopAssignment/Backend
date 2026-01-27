using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.Core
{
    /// <inheritdoc />
    public partial class OrderVat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Product_NonNegative",
                schema: "core",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "VatRate",
                schema: "core",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "TaxTotal",
                schema: "core",
                table: "Orders",
                newName: "VatTotal");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                schema: "core",
                table: "OrderItems",
                newName: "UnitVatAmount");

            migrationBuilder.RenameColumn(
                name: "LineTotal",
                schema: "core",
                table: "OrderItems",
                newName: "UnitPriceIncVat");

            migrationBuilder.AlterColumn<int>(
                name: "VatRate",
                schema: "core",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 25,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,4)",
                oldPrecision: 5,
                oldScale: 4);

            migrationBuilder.AddColumn<decimal>(
                name: "LineTotalExVat",
                schema: "core",
                table: "OrderItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LineTotalIncVat",
                schema: "core",
                table: "OrderItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LineTotalVat",
                schema: "core",
                table: "OrderItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPriceExVat",
                schema: "core",
                table: "OrderItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "VatRatePercent",
                schema: "core",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Product_NonNegative",
                schema: "core",
                table: "Products",
                sql: "[OnHand] >= 0 AND [Reserved] >= 0 AND [LowStockThreshold] >= 0 AND [PriceExVat] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItem_NonNegativeAmounts",
                schema: "core",
                table: "OrderItems",
                sql: "[UnitPriceExVat] >= 0 AND [UnitVatAmount] >= 0 AND [UnitPriceIncVat] >= 0 AND [LineTotalExVat] >= 0 AND [LineTotalVat] >= 0 AND [LineTotalIncVat] >= 0 AND [Quantity] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderItem_VatRatePercent_Allowed",
                schema: "core",
                table: "OrderItems",
                sql: "[VatRatePercent] IN (6, 12, 25)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Product_NonNegative",
                schema: "core",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItem_NonNegativeAmounts",
                schema: "core",
                table: "OrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderItem_VatRatePercent_Allowed",
                schema: "core",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "LineTotalExVat",
                schema: "core",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "LineTotalIncVat",
                schema: "core",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "LineTotalVat",
                schema: "core",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "UnitPriceExVat",
                schema: "core",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "VatRatePercent",
                schema: "core",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "VatTotal",
                schema: "core",
                table: "Orders",
                newName: "TaxTotal");

            migrationBuilder.RenameColumn(
                name: "UnitVatAmount",
                schema: "core",
                table: "OrderItems",
                newName: "UnitPrice");

            migrationBuilder.RenameColumn(
                name: "UnitPriceIncVat",
                schema: "core",
                table: "OrderItems",
                newName: "LineTotal");

            migrationBuilder.AlterColumn<decimal>(
                name: "VatRate",
                schema: "core",
                table: "Products",
                type: "decimal(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 25);

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                schema: "core",
                table: "OrderItems",
                type: "decimal(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Product_NonNegative",
                schema: "core",
                table: "Products",
                sql: "[OnHand] >= 0 AND [Reserved] >= 0 AND [LowStockThreshold] >= 0");
        }
    }
}
