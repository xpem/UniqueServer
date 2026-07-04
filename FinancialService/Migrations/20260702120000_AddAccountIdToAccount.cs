using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialService.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountIdToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "Account",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.Sql(
                "UPDATE `Account` SET `AccountId` = UUID() WHERE `AccountId` IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Account_AccountId_UserId",
                table: "Account",
                columns: new[] { "AccountId", "UserId" },
                unique: true,
                filter: "`AccountId` IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Account_AccountId_UserId",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Account");
        }
    }
}
