using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryRepo.Migrations
{
    /// <inheritdoc />
    public partial class AddItemUserIdIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cobre GetAsync, GetTotalAsync e GetLastPurchaseStores
            // (todas as queries que filtram por UserId e ordenam por CreatedAt)
            migrationBuilder.CreateIndex(
                name: "IX_Item_UserId_CreatedAt",
                table: "Item",
                columns: new[] { "UserId", "CreatedAt" });

            // Cobre GetItemSituationsWithQuantities e GetBySearchAsync com filtro de situação
            migrationBuilder.CreateIndex(
                name: "IX_Item_UserId_ItemSituationId",
                table: "Item",
                columns: new[] { "UserId", "ItemSituationId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Item_UserId_CreatedAt",
                table: "Item");

            migrationBuilder.DropIndex(
                name: "IX_Item_UserId_ItemSituationId",
                table: "Item");
        }
    }
}
