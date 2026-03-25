using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.Core
{
    /// <inheritdoc />
    public partial class CustomRequestInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomRequest",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AttatchemntName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    AttatchemtBlobPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InternalNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomQuote",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomRequestId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CustomerMessage = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    InternalNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubtotalExVat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VatTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalIncVat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomQuote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomQuote_CustomRequest_CustomRequestId",
                        column: x => x.CustomRequestId,
                        principalSchema: "core",
                        principalTable: "CustomRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomQuoteItem",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomQuoteId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    VatRatePercent = table.Column<int>(type: "int", nullable: false),
                    UnitPriceExVat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitVatAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPriceIncVat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotalExVat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotalVat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotalIncVat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomQuoteItem", x => x.Id);
                    table.CheckConstraint("CK_CustomQuoteItem_NonNegativeAmounts", "[UnitPriceExVat] >= 0 AND [UnitVatAmount] >= 0 AND [UnitPriceIncVat] >= 0 AND [LineTotalExVat] >= 0 AND [LineTotalVat] >= 0 AND [LineTotalIncVat] >= 0 AND [Quantity] > 0");
                    table.CheckConstraint("CK_CustomQuoteItem_VatRatePercent_Allowed", "[VatRatePercent] IN (6, 12, 25)");
                    table.ForeignKey(
                        name: "FK_CustomQuoteItem_CustomQuote_CustomQuoteId",
                        column: x => x.CustomQuoteId,
                        principalSchema: "core",
                        principalTable: "CustomQuote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomQuote_CreatedAtUtc",
                schema: "core",
                table: "CustomQuote",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_CustomQuote_CustomRequestId",
                schema: "core",
                table: "CustomQuote",
                column: "CustomRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomQuote_Status",
                schema: "core",
                table: "CustomQuote",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CustomQuoteItem_CustomQuoteId",
                schema: "core",
                table: "CustomQuoteItem",
                column: "CustomQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomRequest_CreatedAtUtc",
                schema: "core",
                table: "CustomRequest",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_CustomRequest_Email",
                schema: "core",
                table: "CustomRequest",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_CustomRequest_Status",
                schema: "core",
                table: "CustomRequest",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomQuoteItem",
                schema: "core");

            migrationBuilder.DropTable(
                name: "CustomQuote",
                schema: "core");

            migrationBuilder.DropTable(
                name: "CustomRequest",
                schema: "core");
        }
    }
}
