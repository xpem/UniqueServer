using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InventoryRepo.Migrations
{
    /// <inheritdoc />
    public partial class AddCategorySubCategoryHistoric : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoryHistoricItemField",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryHistoricItemField", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoryHistoricType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryHistoricType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubCategoryHistoricItemField",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategoryHistoricItemField", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubCategoryHistoricType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategoryHistoricType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoryHistoric",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    CategoryHistoricTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryHistoric", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryHistoric_CategoryHistoricType_CategoryHistoricTypeId",
                        column: x => x.CategoryHistoricTypeId,
                        principalTable: "CategoryHistoricType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryHistoric_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubCategoryHistoric",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SubCategoryId = table.Column<int>(type: "integer", nullable: false),
                    SubCategoryHistoricTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategoryHistoric", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubCategoryHistoric_SubCategoryHistoricType_SubCategoryHist~",
                        column: x => x.SubCategoryHistoricTypeId,
                        principalTable: "SubCategoryHistoricType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubCategoryHistoric_SubCategory_SubCategoryId",
                        column: x => x.SubCategoryId,
                        principalTable: "SubCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CategoryHistoricItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryHistoricItemFieldId = table.Column<int>(type: "integer", nullable: false),
                    UpdatedFrom = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    UpdatedTo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CategoryHistoricId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryHistoricItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryHistoricItem_CategoryHistoricItemField_CategoryHist~",
                        column: x => x.CategoryHistoricItemFieldId,
                        principalTable: "CategoryHistoricItemField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryHistoricItem_CategoryHistoric_CategoryHistoricId",
                        column: x => x.CategoryHistoricId,
                        principalTable: "CategoryHistoric",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubCategoryHistoricItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubCategoryHistoricItemFieldId = table.Column<int>(type: "integer", nullable: false),
                    UpdatedFrom = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    UpdatedTo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    SubCategoryHistoricId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategoryHistoricItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubCategoryHistoricItem_SubCategoryHistoricItemField_SubCat~",
                        column: x => x.SubCategoryHistoricItemFieldId,
                        principalTable: "SubCategoryHistoricItemField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubCategoryHistoricItem_SubCategoryHistoric_SubCategoryHist~",
                        column: x => x.SubCategoryHistoricId,
                        principalTable: "SubCategoryHistoric",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryHistoric_CategoryHistoricTypeId",
                table: "CategoryHistoric",
                column: "CategoryHistoricTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryHistoric_CategoryId",
                table: "CategoryHistoric",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryHistoric_CreatedAt_UserId",
                table: "CategoryHistoric",
                columns: new[] { "CreatedAt", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryHistoricItem_CategoryHistoricId",
                table: "CategoryHistoricItem",
                column: "CategoryHistoricId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryHistoricItem_CategoryHistoricItemFieldId",
                table: "CategoryHistoricItem",
                column: "CategoryHistoricItemFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoryHistoric_CreatedAt_UserId",
                table: "SubCategoryHistoric",
                columns: new[] { "CreatedAt", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoryHistoric_SubCategoryHistoricTypeId",
                table: "SubCategoryHistoric",
                column: "SubCategoryHistoricTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoryHistoric_SubCategoryId",
                table: "SubCategoryHistoric",
                column: "SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoryHistoricItem_SubCategoryHistoricId",
                table: "SubCategoryHistoricItem",
                column: "SubCategoryHistoricId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoryHistoricItem_SubCategoryHistoricItemFieldId",
                table: "SubCategoryHistoricItem",
                column: "SubCategoryHistoricItemFieldId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryHistoricItem");

            migrationBuilder.DropTable(
                name: "SubCategoryHistoricItem");

            migrationBuilder.DropTable(
                name: "CategoryHistoricItemField");

            migrationBuilder.DropTable(
                name: "CategoryHistoric");

            migrationBuilder.DropTable(
                name: "SubCategoryHistoricItemField");

            migrationBuilder.DropTable(
                name: "SubCategoryHistoric");

            migrationBuilder.DropTable(
                name: "CategoryHistoricType");

            migrationBuilder.DropTable(
                name: "SubCategoryHistoricType");
        }
    }
}
