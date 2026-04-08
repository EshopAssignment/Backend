using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.Core
{
    /// <inheritdoc />
    public partial class BlobRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbUrl",
                schema: "core",
                table: "ProductImages",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "AltText",
                schema: "core",
                table: "ProductImages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardUrl",
                schema: "core",
                table: "ProductImages",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LargeUrl",
                schema: "core",
                table: "ProductImages",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OriginalUrl",
                schema: "core",
                table: "ProductImages",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StackUrl",
                schema: "core",
                table: "ProductImages",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardUrl",
                schema: "core",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "LargeUrl",
                schema: "core",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "OriginalUrl",
                schema: "core",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "StackUrl",
                schema: "core",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "Url",
                schema: "core",
                table: "ProductImages");

            migrationBuilder.AlterColumn<string>(
                name: "AltText",
                schema: "core",
                table: "ProductImages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
