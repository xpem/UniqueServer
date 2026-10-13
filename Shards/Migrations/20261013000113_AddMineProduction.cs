using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shards.Migrations
{
    /// <inheritdoc />
    public partial class AddMineProduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductionType",
                table: "Mine",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductionType",
                table: "Mine");
        }
    }
}
