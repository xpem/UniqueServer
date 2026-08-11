using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MobService.Migrations
{
    /// <inheritdoc />
    public partial class AddPetSickness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Sickness",
                table: "Pet",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "SicknessTimer",
                table: "Pet",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sickness",
                table: "Pet");

            migrationBuilder.DropColumn(
                name: "SicknessTimer",
                table: "Pet");
        }
    }
}
