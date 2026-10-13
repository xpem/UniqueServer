using Shards.Config;
using Shards.Model.DTO;

namespace Shards.Rules
{
    public record MissionReward(int Xp, IReadOnlyList<OreAmount> Ores);

    public record MissionDefinition(MissionType Type, string Name, string Description, int Goal, MissionReward Reward);

    /// <summary>
    /// Definição das missões (no código). O progresso de cada jogador fica em PlayerMissionDTO.
    /// A ordem da lista é a ordem da cadeia: ao resgatar uma missão, a seguinte é ativada.
    /// </summary>
    public static class MissionCatalog
    {
        public static IReadOnlyList<MissionDefinition> All { get; } =
        [
            new(MissionType.FirstPickaxe, "Primeira Picaretagem", "Gaste 10 pontos de energia minerando.", 10,
                new MissionReward(15, [])),

            new(MissionType.Specialization, "Especialização", "Abra o perfil e distribua seu primeiro ponto de habilidade.", 1,
                new MissionReward(0, [new OreAmount { Ore = OreType.Quartz, Quantity = 25 }])),

            new(MissionType.OperationalExpansion, "Expansão Operacional", "Junte 200 Quartzo e desbloqueie a segunda mina.", 1,
                new MissionReward(0, [])),

            new(MissionType.FirstGem, "Primeira Gema", "Escolha uma gema para uma de suas minas produzir.", 1,
                new MissionReward(50, [])),
        ];

        public static MissionDefinition First => All[0];

        public static MissionDefinition Get(MissionType type) =>
            All.FirstOrDefault(m => m.Type == type) ?? throw new ArgumentOutOfRangeException(nameof(type), type, "Missão inexistente no catálogo.");

        /// <summary>Próxima missão da cadeia, ou null se for a última.</summary>
        public static MissionDefinition? Next(MissionType type)
        {
            for (int i = 0; i < All.Count - 1; i++)
                if (All[i].Type == type) return All[i + 1];

            return null;
        }
    }
}
