using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialService.Migrations
{
    /// <inheritdoc />
    public partial class MultiAccountExpansion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Handle existing NULL UserId values before making column non-nullable
            migrationBuilder.Sql("UPDATE `Account` SET `UserId` = 0 WHERE `UserId` IS NULL;");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Account",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Account",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Conta Principal")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Account",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentBalance",
                table: "Account",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeInGeneralBalance",
                table: "Account",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "Inactive",
                table: "Account",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentBalance",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "Inactive",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "IncludeInGeneralBalance",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Account");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Account",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
