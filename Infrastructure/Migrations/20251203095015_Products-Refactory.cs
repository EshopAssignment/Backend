using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductsRefactory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems");

            if (ColumnExists(migrationBuilder, "Products", "Price"))
            {
                migrationBuilder.DropColumn(
                    name: "Price",
                    table: "Products");
            }

            migrationBuilder.AddColumn<decimal>(
                name: "PriceExVat",
                table: "Products",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "OnHand",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Reserved",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LowStockThreshold",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 20);

            migrationBuilder.Sql("""
IF COL_LENGTH('Products', 'StockQuantity') IS NOT NULL
BEGIN
    UPDATE p SET p.OnHand = ISNULL(p.StockQuantity, 0)
    FROM Products p;

    ALTER TABLE Products DROP COLUMN StockQuantity;
END
""");

            migrationBuilder.AddColumn<int>(
                name: "Condition_Int",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 1); 

            migrationBuilder.Sql("""
UPDATE Products
SET Condition_Int = CASE UPPER(LTRIM(RTRIM(CAST(Condition AS nvarchar(50)))))
    WHEN 'NY' THEN 1
    WHEN 'NEW' THEN 1
    WHEN 'USED' THEN 2
    WHEN 'BEGAGNAD' THEN 2
    WHEN 'REFURBISHED' THEN 3
    WHEN 'RENOVERAD' THEN 3
    ELSE 1
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH('Products', 'Condition') IS NOT NULL
    ALTER TABLE Products DROP COLUMN Condition;
""");

            migrationBuilder.RenameColumn(
                name: "Condition_Int",
                table: "Products",
                newName: "Condition");

            migrationBuilder.AddColumn<int>(
                name: "PalletType_Int",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 1); 

            migrationBuilder.Sql("""
UPDATE Products
SET PalletType_Int = CASE UPPER(LTRIM(RTRIM(CAST(PalletType AS nvarchar(50)))))
    WHEN 'EUR' THEN 1
    WHEN 'EURO' THEN 1
    WHEN 'EUROPALLET' THEN 1
    WHEN 'HALVPALL' THEN 2
    WHEN 'HALFPALLET' THEN 2
    WHEN 'INDUSTRIALPALLET' THEN 3
    WHEN 'INDUSTRIAL' THEN 3
    WHEN 'CUSTOMPALLET' THEN 4
    WHEN 'CUSTOM' THEN 4
    WHEN 'SPECIALPALLET' THEN 5
    WHEN 'SPECIAL' THEN 5
    ELSE 6
END
""");

            migrationBuilder.Sql("""
IF COL_LENGTH('Products', 'PalletType') IS NOT NULL
    ALTER TABLE Products DROP COLUMN PalletType;
""");

            migrationBuilder.RenameColumn(
                name: "PalletType_Int",
                table: "Products",
                newName: "PalletType");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Products",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Product_NonNegative",
                table: "Products",
                sql: "[OnHand] >= 0 AND [Reserved] >= 0 AND [LowStockThreshold] >= 0");

            migrationBuilder.Sql("""
-- rensa ev. dubblettindex från tidigare körningar
IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Slug_Unique' AND object_id = OBJECT_ID('Products'))
    DROP INDEX IX_Products_Slug_Unique ON Products;
IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Sku_Unique' AND object_id = OBJECT_ID('Products'))
    DROP INDEX IX_Products_Sku_Unique ON Products;

UPDATE p SET
  Sku  = COALESCE(Sku, CONCAT('SKU-', RIGHT(CONVERT(varchar(32), NEWID()), 8))),
  Slug = COALESCE(Slug,
                  LOWER(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Name, '(', ''), ')', ''), '  ', ' '), ' ', '-'), 'å', 'a')))
FROM Products p
WHERE p.Sku IS NULL OR p.Slug IS NULL;
""");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug_Unique",
                table: "Products",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Sku_Unique",
                table: "Products",
                column: "Sku",
                unique: true,
                filter: "[Sku] IS NOT NULL");

            migrationBuilder.CreateTable(
                name: "StockReservations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CartId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockReservations_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_ProductId_Status",
                table: "StockReservations",
                columns: new[] { "ProductId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_IdempotencyKey",
                table: "StockReservations",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems");

            migrationBuilder.DropTable(
                name: "StockReservations");

            migrationBuilder.DropIndex(
                name: "IX_Products_Slug_Unique",
                table: "Products");
            migrationBuilder.DropIndex(
                name: "IX_Products_Sku_Unique",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Product_NonNegative",
                table: "Products");

            migrationBuilder.DropColumn(name: "RowVersion", table: "Products");
            migrationBuilder.DropColumn(name: "PriceExVat", table: "Products");
            migrationBuilder.DropColumn(name: "OnHand", table: "Products");
            migrationBuilder.DropColumn(name: "Reserved", table: "Products");
            migrationBuilder.DropColumn(name: "LowStockThreshold", table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Condition_Str",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "New");

            migrationBuilder.Sql("""
UPDATE Products
SET Condition_Str = CASE Condition
    WHEN 1 THEN 'New'
    WHEN 2 THEN 'Used'
    WHEN 3 THEN 'Refurbished'
    ELSE 'New'
END
""");

            migrationBuilder.AddColumn<string>(
                name: "PalletType_Str",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "EuroPallet");

            migrationBuilder.Sql("""
UPDATE Products
SET PalletType_Str = CASE PalletType
    WHEN 1 THEN 'EuroPallet'
    WHEN 2 THEN 'HalfPallet'
    WHEN 3 THEN 'IndustrialPallet'
    WHEN 4 THEN 'CustomPallet'
    WHEN 5 THEN 'SpecialPallet'
    ELSE 'Other'
END
""");

            migrationBuilder.Sql("ALTER TABLE Products DROP COLUMN Condition;");
            migrationBuilder.Sql("ALTER TABLE Products DROP COLUMN PalletType;");

            migrationBuilder.RenameColumn(
                name: "Condition_Str",
                table: "Products",
                newName: "Condition");

            migrationBuilder.RenameColumn(
                name: "PalletType_Str",
                table: "Products",
                newName: "PalletType");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        private static bool ColumnExists(MigrationBuilder m, string table, string column)
            => true;
    }
}
