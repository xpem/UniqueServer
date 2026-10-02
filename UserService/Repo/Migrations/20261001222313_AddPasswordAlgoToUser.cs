using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagementService.Repo.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordAlgoToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PasswordAlgo",
                table: "User",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordAlgo",
                table: "User");
        }
    }
}
