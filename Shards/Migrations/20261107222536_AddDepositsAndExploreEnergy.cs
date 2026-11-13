using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Shards.Migrations
{
    /// <inheritdoc />
    public partial class AddDepositsAndExploreEnergy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExploreEnergy",
                table: "Player",
                type: "integer",
                nullable: false,
                defaultValue: 30); // quem já joga começa com a barra de exploração cheia

            migrationBuilder.AddColumn<int>(
                name: "ExploreMaxEnergy",
                table: "Player",
                type: "integer",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastExploreEnergyUpdateUtc",
                table: "Player",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()"); // com a barra cheia o instante é irrelevante; evita o -infinity em quem já joga

            migrationBuilder.CreateTable(
                name: "Deposit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    Slot = table.Column<int>(type: "integer", nullable: false),
                    WeekStart = table.Column<DateOnly>(type: "date", nullable: false),
                    Element = table.Column<int>(type: "integer", nullable: false),
                    DefenseMax = table.Column<int>(type: "integer", nullable: false),
                    DefenseCurrent = table.Column<int>(type: "integer", nullable: false),
                    StockMax = table.Column<int>(type: "integer", nullable: false),
                    StockRemaining = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RerollCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deposit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deposit_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Deposit_PlayerId_WeekStart_Slot",
                table: "Deposit",
                columns: new[] { "PlayerId", "WeekStart", "Slot" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Deposit");

            migrationBuilder.DropColumn(
                name: "ExploreEnergy",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "ExploreMaxEnergy",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "LastExploreEnergyUpdateUtc",
                table: "Player");
        }
    }
}
