using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialService.Migrations
{
    /// <inheritdoc />
    public partial class _202605131 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMainTransactionCategory",
                table: "TransactionCategory",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ParentTransactionCategoryId",
                table: "TransactionCategory",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCategory_Name",
                table: "TransactionCategory",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransactionCategory_Name",
                table: "TransactionCategory");

            migrationBuilder.DropColumn(
                name: "IsMainTransactionCategory",
                table: "TransactionCategory");

            migrationBuilder.DropColumn(
                name: "ParentTransactionCategoryId",
                table: "TransactionCategory");
        }
    }
}
