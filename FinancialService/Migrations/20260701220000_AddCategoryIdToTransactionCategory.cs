using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialService.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryIdToTransactionCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "TransactionCategory",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.Sql(
                "UPDATE `TransactionCategory` SET `CategoryId` = UUID() WHERE `CategoryId` IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategory_CategoryId_UserId",
                table: "TransactionCategory",
                columns: new[] { "CategoryId", "UserId" },
                unique: true,
                filter: "`CategoryId` IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransactionCategory_CategoryId_UserId",
                table: "TransactionCategory");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "TransactionCategory");
        }
    }
}
