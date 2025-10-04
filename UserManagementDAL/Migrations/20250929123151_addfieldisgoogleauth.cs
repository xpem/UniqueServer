using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserDAL.Migrations
{
    /// <inheritdoc />
    public partial class addfieldisgoogleauth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserHistoric_UserHistoricType_UserHistoricTypeId",
                table: "UserHistoric");

            migrationBuilder.DropTable(
                name: "UserHistoricType");

            migrationBuilder.DropIndex(
                name: "IX_UserHistoric_UserHistoricTypeId",
                table: "UserHistoric");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "User",
                type: "varchar(350)",
                maxLength: 350,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(350)",
                oldMaxLength: 350)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsGoogleAuth",
                table: "User",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGoogleAuth",
                table: "User");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Password",
                keyValue: null,
                column: "Password",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "User",
                type: "varchar(350)",
                maxLength: 350,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(350)",
                oldMaxLength: 350,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserHistoricType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHistoricType", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
    }
}
