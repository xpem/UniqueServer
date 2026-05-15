using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialService.Migrations
{
    /// <inheritdoc />
    public partial class _202605132 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransactionCategory_Name",
                table: "TransactionCategory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategory_Name",
                table: "TransactionCategory",
                column: "Name",
                unique: true);
        }
    }
}
