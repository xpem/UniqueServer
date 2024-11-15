using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryDbContextDAL.Migrations
{
    /// <inheritdoc />
    public partial class criacampoversionnasubcategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "SubCategory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "ItemSituation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Category",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "AcquisitionType",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "SubCategory");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ItemSituation");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "AcquisitionType");
        }
    }
}
