using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserDAL.Migrations
{
    /// <inheritdoc />
    public partial class Ajustenaforeignkeydouserhistoric : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HistoricTypeId",
                table: "UserHistoric",
                newName: "UserHistoricTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHistoric_UserHistoricTypeId",
                table: "UserHistoric",
                column: "UserHistoricTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserHistoric_UserHistoricType_UserHistoricTypeId",
                table: "UserHistoric",
                column: "UserHistoricTypeId",
                principalTable: "UserHistoricType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserHistoric_UserHistoricType_UserHistoricTypeId",
                table: "UserHistoric");

            migrationBuilder.DropIndex(
                name: "IX_UserHistoric_UserHistoricTypeId",
                table: "UserHistoric");

            migrationBuilder.RenameColumn(
                name: "UserHistoricTypeId",
                table: "UserHistoric",
                newName: "HistoricTypeId");
        }
    }
}
