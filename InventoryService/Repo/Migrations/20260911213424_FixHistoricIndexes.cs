using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryRepo.Migrations
{
    /// <inheritdoc />
    public partial class FixHistoricIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubCategoryHistoric_CreatedAt_UserId",
                table: "SubCategoryHistoric");

            migrationBuilder.DropIndex(
                name: "IX_SubCategoryHistoric_SubCategoryId",
                table: "SubCategoryHistoric");

            migrationBuilder.DropIndex(
                name: "IX_ItemHistoric_CreatedAt_UserId",
                table: "ItemHistoric");

            migrationBuilder.DropIndex(
                name: "IX_ItemHistoric_ItemId",
                table: "ItemHistoric");

            migrationBuilder.DropIndex(
                name: "IX_CategoryHistoric_CategoryId",
                table: "CategoryHistoric");

            migrationBuilder.DropIndex(
                name: "IX_CategoryHistoric_CreatedAt_UserId",
                table: "CategoryHistoric");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoryHistoric_SubCategoryId_UserId_CreatedAt",
                table: "SubCategoryHistoric",
                columns: new[] { "SubCategoryId", "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemHistoric_ItemId_UserId_CreatedAt",
                table: "ItemHistoric",
                columns: new[] { "ItemId", "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryHistoric_CategoryId_UserId_CreatedAt",
                table: "CategoryHistoric",
                columns: new[] { "CategoryId", "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubCategoryHistoric_SubCategoryId_UserId_CreatedAt",
                table: "SubCategoryHistoric");

            migrationBuilder.DropIndex(
                name: "IX_ItemHistoric_ItemId_UserId_CreatedAt",
                table: "ItemHistoric");

            migrationBuilder.DropIndex(
                name: "IX_CategoryHistoric_CategoryId_UserId_CreatedAt",
                table: "CategoryHistoric");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoryHistoric_CreatedAt_UserId",
                table: "SubCategoryHistoric",
                columns: new[] { "CreatedAt", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoryHistoric_SubCategoryId",
                table: "SubCategoryHistoric",
                column: "SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemHistoric_CreatedAt_UserId",
                table: "ItemHistoric",
                columns: new[] { "CreatedAt", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemHistoric_ItemId",
                table: "ItemHistoric",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryHistoric_CategoryId",
                table: "CategoryHistoric",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryHistoric_CreatedAt_UserId",
                table: "CategoryHistoric",
                columns: new[] { "CreatedAt", "UserId" });
        }
    }
}
