using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserDAL.Migrations
{
    /// <inheritdoc />
    public partial class Campodeusuarionohistoricodousuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "UserHistoric",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserHistoric_UserId",
                table: "UserHistoric",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserHistoric_User_UserId",
                table: "UserHistoric",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserHistoric_User_UserId",
                table: "UserHistoric");

            migrationBuilder.DropIndex(
                name: "IX_UserHistoric_UserId",
                table: "UserHistoric");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserHistoric");
        }
    }
}
