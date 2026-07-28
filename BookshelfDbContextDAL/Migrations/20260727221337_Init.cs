using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BookshelfRepo.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Book",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    BookId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Subtitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Authors = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Volume = table.Column<int>(type: "integer", maxLength: 3, nullable: true),
                    Pages = table.Column<int>(type: "integer", maxLength: 3, nullable: true),
                    Year = table.Column<int>(type: "integer", maxLength: 4, nullable: true),
                    Status = table.Column<int>(type: "integer", maxLength: 2, nullable: false),
                    Genre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Isbn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Cover = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    GoogleId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Score = table.Column<int>(type: "integer", maxLength: 1, nullable: true),
                    Comment = table.Column<string>(type: "character varying(350)", maxLength: 350, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Inactive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Book", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookHistoricItemField",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookHistoricItemField", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookHistoricType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookHistoricType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookHistoric",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    BookId = table.Column<int>(type: "integer", nullable: false),
                    BookHistoricTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookHistoric", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookHistoric_BookHistoricType_BookHistoricTypeId",
                        column: x => x.BookHistoricTypeId,
                        principalTable: "BookHistoricType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookHistoric_Book_BookId",
                        column: x => x.BookId,
                        principalTable: "Book",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookHistoricItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookHistoricItemFieldId = table.Column<int>(type: "integer", nullable: false),
                    UpdatedFrom = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    UpdatedTo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    BookHistoricId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookHistoricItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookHistoricItem_BookHistoricItemField_BookHistoricItemFiel~",
                        column: x => x.BookHistoricItemFieldId,
                        principalTable: "BookHistoricItemField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookHistoricItem_BookHistoric_BookHistoricId",
                        column: x => x.BookHistoricId,
                        principalTable: "BookHistoric",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookHistoric_BookHistoricTypeId",
                table: "BookHistoric",
                column: "BookHistoricTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BookHistoric_BookId",
                table: "BookHistoric",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookHistoric_CreatedAt_UserId",
                table: "BookHistoric",
                columns: new[] { "CreatedAt", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_BookHistoricItem_BookHistoricId",
                table: "BookHistoricItem",
                column: "BookHistoricId");

            migrationBuilder.CreateIndex(
                name: "IX_BookHistoricItem_BookHistoricItemFieldId",
                table: "BookHistoricItem",
                column: "BookHistoricItemFieldId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookHistoricItem");

            migrationBuilder.DropTable(
                name: "BookHistoricItemField");

            migrationBuilder.DropTable(
                name: "BookHistoric");

            migrationBuilder.DropTable(
                name: "BookHistoricType");

            migrationBuilder.DropTable(
                name: "Book");
        }
    }
}
