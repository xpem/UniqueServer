using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryDbContextDAL.Migrations
{
    /// <inheritdoc />
    public partial class criacampoinativo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Inactive",
                table: "SubCategory",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Inactive",
                table: "ItemSituation",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Inactive",
                table: "Category",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Inactive",
                table: "AcquisitionType",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Inactive",
                table: "SubCategory");

            migrationBuilder.DropColumn(
                name: "Inactive",
                table: "ItemSituation");

            migrationBuilder.DropColumn(
                name: "Inactive",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "Inactive",
                table: "AcquisitionType");
        }
    }
}
