using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryRepo.Migrations
{
    /// <inheritdoc />
    public partial class AddItemParentAssociation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentItemId",
                table: "Item",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Item_ParentItemId",
                table: "Item",
                column: "ParentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_UserId_ParentItemId",
                table: "Item",
                columns: new[] { "UserId", "ParentItemId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Item_Item_ParentItemId",
                table: "Item",
                column: "ParentItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Novo campo rastreado no histórico de item (ver ItemHistoricService.Field(10, ...)).
            // Inserido explicitamente aqui porque a seed de app (InventoryInitializeDB) só roda em tabela vazia.
            migrationBuilder.InsertData(
                table: "ItemHistoricItemField",
                columns: new[] { "Id", "Name" },
                values: new object[] { 10, "Item Associado" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ItemHistoricItemField",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DropForeignKey(
                name: "FK_Item_Item_ParentItemId",
                table: "Item");

            migrationBuilder.DropIndex(
                name: "IX_Item_ParentItemId",
                table: "Item");

            migrationBuilder.DropIndex(
                name: "IX_Item_UserId_ParentItemId",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "ParentItemId",
                table: "Item");
        }
    }
}
