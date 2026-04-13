using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

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
                name: "EmailOutbox",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    To = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HtmlBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastAttempt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NextAttempt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailOutbox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerFirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomerLastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomerEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CustomerPhoneNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShippingStreet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ShippingCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShippingPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ShippingCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OrderStatus = table.Column<int>(type: "int", nullable: false),
                    ShippingMethod = table.Column<int>(type: "int", nullable: false),
                    ShippingCarrier = table.Column<int>(type: "int", nullable: false),
                    ServicePointId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServicePointName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServicePointAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ProductsSubtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShippingCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VatTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CartId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FulfillmentStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FulfilledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FulfillmentNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Payment_Status = table.Column<int>(type: "int", nullable: false),
                    Payment_PaymentIntentId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Payment_LatestChargeId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Payment_PaymentMethodType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Payment_Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Payment_AmountAuthorized = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payment_AmountCaptured = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payment_AmountRefunded = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payment_AuthorizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Payment_CapturedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Payment_RefundedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PublichedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublishAttempts = table.Column<int>(type: "int", nullable: false),
                    LastAttemptUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriceExVat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VatRate = table.Column<int>(type: "int", nullable: false, defaultValue: 25),
                    Condition = table.Column<int>(type: "int", nullable: false),
                    PalletType = table.Column<int>(type: "int", nullable: false),
                    OnHand = table.Column<int>(type: "int", nullable: false),
                    Reserved = table.Column<int>(type: "int", nullable: false),
                    LowStockThreshold = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.CheckConstraint("CK_Product_NonNegative", "[OnHand] >= 0 AND [Reserved] >= 0 AND [LowStockThreshold] >= 0 AND [PriceExVat] >= 0");
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
                name: "OrderItems",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VatRatePercent = table.Column<int>(type: "int", nullable: false),
                    UnitPriceExVat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitVatAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPriceIncVat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    LineTotalExVat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotalVat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotalIncVat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.CheckConstraint("CK_OrderItem_NonNegativeAmounts", "[UnitPriceExVat] >= 0 AND [UnitVatAmount] >= 0 AND [UnitPriceIncVat] >= 0 AND [LineTotalExVat] >= 0 AND [LineTotalVat] >= 0 AND [LineTotalIncVat] >= 0 AND [Quantity] > 0");
                    table.CheckConstraint("CK_OrderItem_VatRatePercent_Allowed", "[VatRatePercent] IN (6, 12, 25)");
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "core",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "core",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    OriginalUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    LargeUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    CardUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    StackUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    ThumbUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AltText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "core",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockReservations",
                schema: "core",
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
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PaymentIntentId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockReservations_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "core",
                        principalTable: "Products",
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

            migrationBuilder.CreateIndex(
                name: "IX_EmailOutbox_CorrelationId",
                schema: "core",
                table: "EmailOutbox",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailOutbox_Status_NextAttempt",
                schema: "core",
                table: "EmailOutbox",
                columns: new[] { "Status", "NextAttempt" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                schema: "core",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                schema: "core",
                table: "OrderItems",
                column: "ProductId");

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
                name: "IX_Orders_OrderNumber",
                schema: "core",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderStatus",
                schema: "core",
                table: "Orders",
                column: "OrderStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                schema: "core",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_CorrelationId",
                schema: "core",
                table: "OutboxMessages",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_PublichedAtUtc_CreatedAtUtc",
                schema: "core",
                table: "OutboxMessages",
                columns: new[] { "PublichedAtUtc", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_IsPrimary",
                schema: "core",
                table: "ProductImages",
                columns: new[] { "ProductId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_SortOrder",
                schema: "core",
                table: "ProductImages",
                columns: new[] { "ProductId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_Name",
                schema: "core",
                table: "Products",
                columns: new[] { "IsActive", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_Sku",
                schema: "core",
                table: "Products",
                columns: new[] { "IsActive", "Sku" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_Slug",
                schema: "core",
                table: "Products",
                columns: new[] { "IsActive", "Slug" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Sku",
                schema: "core",
                table: "Products",
                column: "Sku",
                unique: true,
                filter: "[Sku] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                schema: "core",
                table: "Products",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_Cart_Status",
                schema: "core",
                table: "StockReservations",
                columns: new[] { "CartId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_IdempotencyKey",
                schema: "core",
                table: "StockReservations",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_ProductId_Status",
                schema: "core",
                table: "StockReservations",
                columns: new[] { "ProductId", "Status" });

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
            migrationBuilder.DropTable(
                name: "CustomQuoteItem",
                schema: "core");

            migrationBuilder.DropTable(
                name: "EmailOutbox",
                schema: "core");

            migrationBuilder.DropTable(
                name: "OrderItems",
                schema: "core");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "core");

            migrationBuilder.DropTable(
                name: "ProductImages",
                schema: "core");

            migrationBuilder.DropTable(
                name: "StockReservations",
                schema: "core");

            migrationBuilder.DropTable(
                name: "CustomQuote",
                schema: "core");

            migrationBuilder.DropTable(
                name: "Orders",
                schema: "core");

            migrationBuilder.DropTable(
                name: "Products",
                schema: "core");

            migrationBuilder.DropTable(
                name: "CustomRequest",
                schema: "core");
        }
    }
}
