using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.Core
{
    /// <inheritdoc />
    public partial class EmailOutBoxInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    NextAttempt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailOutbox", x => x.Id);
                });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailOutbox",
                schema: "core");
        }
    }
}
