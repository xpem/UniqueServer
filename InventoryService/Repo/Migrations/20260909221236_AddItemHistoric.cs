using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InventoryRepo.Migrations
{
    /// <inheritdoc />
    public partial class AddItemHistoric : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemHistoricItemField",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemHistoricItemField", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemHistoricType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemHistoricType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemHistoric",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ItemHistoricTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemHistoric", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemHistoric_ItemHistoricType_ItemHistoricTypeId",
                        column: x => x.ItemHistoricTypeId,
                        principalTable: "ItemHistoricType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemHistoric_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemHistoricItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemHistoricItemFieldId = table.Column<int>(type: "integer", nullable: false),
                    UpdatedFrom = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    UpdatedTo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ItemHistoricId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemHistoricItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemHistoricItem_ItemHistoricItemField_ItemHistoricItemFiel~",
                        column: x => x.ItemHistoricItemFieldId,
                        principalTable: "ItemHistoricItemField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemHistoricItem_ItemHistoric_ItemHistoricId",
                        column: x => x.ItemHistoricId,
                        principalTable: "ItemHistoric",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemHistoric_CreatedAt_UserId",
                table: "ItemHistoric",
                columns: new[] { "CreatedAt", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemHistoric_ItemHistoricTypeId",
                table: "ItemHistoric",
                column: "ItemHistoricTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemHistoric_ItemId",
                table: "ItemHistoric",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemHistoricItem_ItemHistoricId",
                table: "ItemHistoricItem",
                column: "ItemHistoricId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemHistoricItem_ItemHistoricItemFieldId",
                table: "ItemHistoricItem",
                column: "ItemHistoricItemFieldId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemHistoricItem");

            migrationBuilder.DropTable(
                name: "ItemHistoricItemField");

            migrationBuilder.DropTable(
                name: "ItemHistoric");

            migrationBuilder.DropTable(
                name: "ItemHistoricType");
        }
    }
}
