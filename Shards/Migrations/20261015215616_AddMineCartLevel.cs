using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shards.Migrations
{
    /// <inheritdoc />
    public partial class AddMineCartLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CartLevel",
                table: "Mine",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CartLevel",
                table: "Mine");
        }
    }
}
