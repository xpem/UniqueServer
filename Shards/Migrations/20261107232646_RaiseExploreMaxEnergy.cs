using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shards.Migrations
{
    /// <inheritdoc />
    public partial class RaiseExploreMaxEnergy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // o teto da barra de exploração passou de 30 para 72 (um dia cheio de regeneração): quem já joga ganha o teto novo.
            // A energia atual não muda; a barra enche no ritmo normal (e quem estava cheio, com o relógio parado, vê o ganho na próxima leitura).
            migrationBuilder.Sql("UPDATE \"Player\" SET \"ExploreMaxEnergy\" = 72 WHERE \"ExploreMaxEnergy\" = 30;");

            // o default das colunas também acompanha o teto novo (só vale para inserts fora do jogo; o jogo grava os valores da configuração)
            migrationBuilder.AlterColumn<int>(name: "ExploreEnergy", table: "Player", type: "integer", nullable: false, defaultValue: 72,
                oldClrType: typeof(int), oldType: "integer", oldDefaultValue: 30);
            migrationBuilder.AlterColumn<int>(name: "ExploreMaxEnergy", table: "Player", type: "integer", nullable: false, defaultValue: 72,
                oldClrType: typeof(int), oldType: "integer", oldDefaultValue: 30);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(name: "ExploreEnergy", table: "Player", type: "integer", nullable: false, defaultValue: 30,
                oldClrType: typeof(int), oldType: "integer", oldDefaultValue: 72);
            migrationBuilder.AlterColumn<int>(name: "ExploreMaxEnergy", table: "Player", type: "integer", nullable: false, defaultValue: 30,
                oldClrType: typeof(int), oldType: "integer", oldDefaultValue: 72);

            // volta ao teto antigo, sem deixar a energia acima dele
            migrationBuilder.Sql("UPDATE \"Player\" SET \"ExploreMaxEnergy\" = 30, \"ExploreEnergy\" = LEAST(\"ExploreEnergy\", 30) WHERE \"ExploreMaxEnergy\" = 72;");
        }
    }
}
