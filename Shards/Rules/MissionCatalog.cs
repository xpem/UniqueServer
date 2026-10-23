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

            new(MissionType.OperationalExpansion, "Expansão Operacional", "Junte 200 Quartzo, libere um espaço de mina com um ponto de habilidade em MineSlots e compre a segunda mina.", 1,
                new MissionReward(0, [])),

            new(MissionType.FirstGem, "Primeira Gema", "Escolha uma gema para uma de suas minas produzir.", 1,
                new MissionReward(50, [])),

            new(MissionType.CartTraining, "Treinamento de Vagonete", "Gaste um ponto de habilidade em Vagonete (CartCapacity).", 1,
                new MissionReward(20, [])),

            new(MissionType.CartUpgrade, "Vagonete Ampliado", "Amplie o vagonete de uma de suas minas.", 1,
                new MissionReward(30, [new OreAmount { Ore = OreType.Quartz, Quantity = 50 }])),

            new(MissionType.OpenWorkshop, "Abrir a Oficina", "Gaste um ponto de habilidade em Oficina (Workshop).", 1,
                new MissionReward(30, [new OreAmount { Ore = OreType.Quartz, Quantity = 100 }])),

            new(MissionType.FirstPolish, "Primeira Lapidação", "Pole uma gema na Oficina.", 1,
                new MissionReward(50, [new OreAmount { Ore = OreType.Quartz, Quantity = 600 }])),

            new(MissionType.FirstFacet, "Gema Facetada", "Faceteie uma gema na Oficina.", 1,
                new MissionReward(100, [])),

            new(MissionType.Diversification, "Diversificação", "Tenha duas minas produzindo gemas diferentes.", 1,
                new MissionReward(30, [new OreAmount { Ore = OreType.Quartz, Quantity = 300 }])),

            new(MissionType.Collector, "Colecionador", "Obtenha as três gemas: Rubi, Safira e Esmeralda (em qualquer estágio).", 1,
                new MissionReward(20, [new OreAmount { Ore = OreType.Quartz, Quantity = 200 }])),

            new(MissionType.PolishBatch, "Polimento em Lote", "Pole 10 gemas na Oficina.", 10,
                new MissionReward(40, [new OreAmount { Ore = OreType.Quartz, Quantity = 500 }])),

            new(MissionType.FullSet, "Conjunto Completo", "Tenha uma gema facetada de cada tipo: Rubi, Safira e Esmeralda.", 1,
                new MissionReward(60, [new OreAmount { Ore = OreType.Quartz, Quantity = 500 }])),

            new(MissionType.FacetBatch, "Mestre Lapidador", "Faceteie 5 gemas na Oficina.", 5,
                new MissionReward(80, [new OreAmount { Ore = OreType.Quartz, Quantity = 1000 }])),

            new(MissionType.ThirdMine, "Terceira Mina", "Tenha três minas.", 1,
                new MissionReward(30, [new OreAmount { Ore = OreType.Quartz, Quantity = 300 }])),

            new(MissionType.FullCart, "Vagonete Cheio", "Esvazie um vagonete que chegou à capacidade máxima.", 1,
                new MissionReward(15, [])),

            new(MissionType.DoubleLuck, "Dupla Sorte", "Leve a habilidade DoubleDrop ao nível 3.", 1,
                new MissionReward(30, [])),
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
