using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MobService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VariantIndex = table.Column<int>(type: "integer", nullable: false),
                    Phase = table.Column<int>(type: "integer", nullable: false),
                    PhaseStart = table.Column<long>(type: "bigint", nullable: false),
                    BornAt = table.Column<long>(type: "bigint", nullable: false),
                    Hunger = table.Column<int>(type: "integer", nullable: false),
                    Energy = table.Column<int>(type: "integer", nullable: false),
                    Health = table.Column<int>(type: "integer", nullable: false),
                    Joy = table.Column<int>(type: "integer", nullable: false),
                    PoopCount = table.Column<int>(type: "integer", nullable: false),
                    BowlPortions = table.Column<int>(type: "integer", nullable: false),
                    Toys = table.Column<int>(type: "integer", nullable: false),
                    IsDead = table.Column<bool>(type: "boolean", nullable: false),
                    DeathCause = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pet", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pet_UserId",
                table: "Pet",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Pet_UserId_IsDead",
                table: "Pet",
                columns: new[] { "UserId", "IsDead" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pet");
        }
    }
}
