using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobService.Migrations
{
    /// <inheritdoc />
    public partial class Addhouseoption3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HouseCharges",
                table: "Pet",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HouseCharges",
                table: "Pet");
        }
    }
}
