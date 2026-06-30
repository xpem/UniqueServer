using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialService.Migrations
{
    /// <inheritdoc />
    public partial class _202606301 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "Transaction",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_TransactionId_UserId",
                table: "Transaction",
                columns: new[] { "TransactionId", "UserId" },
                unique: true,
                filter: "`TransactionId` IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transaction_TransactionId_UserId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Transaction");
        }
    }
}
