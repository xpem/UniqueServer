using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shards.Migrations
{
    /// <inheritdoc />
    public partial class AddItemStage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryItem_PlayerId_OreType",
                table: "InventoryItem");

            migrationBuilder.AddColumn<int>(
                name: "Stage",
                table: "InventoryItem",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItem_PlayerId_OreType_Stage",
                table: "InventoryItem",
                columns: new[] { "PlayerId", "OreType", "Stage" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryItem_PlayerId_OreType_Stage",
                table: "InventoryItem");

            migrationBuilder.DropColumn(
                name: "Stage",
                table: "InventoryItem");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItem_PlayerId_OreType",
                table: "InventoryItem",
                columns: new[] { "PlayerId", "OreType" },
                unique: true);
        }
    }
}
