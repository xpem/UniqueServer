using Shards.Config;
using Shards.Model.DTO;

namespace Shards.Rules
{
    public record DailyMissionDefinition(DailyMissionType Type, string Name, string Description, int Goal, MissionReward Reward);

    /// <summary>
    /// Pool das missões diárias (no código). O progresso de cada jogador/dia fica em DailyMissionDTO.
    /// Recompensas só em XP e quartzo bruto, sempre abaixo do que o jogo rende por si: a diária guia o jogador, não o substitui.
    /// </summary>
    public static class DailyMissionCatalog
    {
        public static IReadOnlyList<DailyMissionDefinition> All { get; } =
        [
            new(DailyMissionType.Login, "Presença", "Entre no jogo hoje.", 1,
                new MissionReward(5, [Quartz(20)])),

            new(DailyMissionType.Energy, "Mão na Massa", "Gaste 50 pontos de energia minerando.", 50,
                new MissionReward(10, [Quartz(40)])),

            new(DailyMissionType.Cart, "Coleta do Dia", "Esvazie um vagonete.", 1,
                new MissionReward(5, [Quartz(40)])),

            new(DailyMissionType.Quartz, "Quartzo do Dia", "Obtenha 100 de quartzo minerando ou coletando.", 100,
                new MissionReward(10, [Quartz(40)])),

            new(DailyMissionType.Gems, "Garimpo de Gemas", "Obtenha 20 gemas brutas minerando ou coletando.", 20,
                new MissionReward(10, [Quartz(60)])),

            new(DailyMissionType.Polish, "Polimento do Dia", "Pole 2 gemas na Oficina.", 2,
                new MissionReward(15, [Quartz(100)])),

            new(DailyMissionType.Facet, "Facetamento do Dia", "Faceteie uma gema na Oficina.", 1,
                new MissionReward(20, [Quartz(150)])),
        ];

        public static DailyMissionDefinition Get(DailyMissionType type) =>
            All.FirstOrDefault(m => m.Type == type) ?? throw new ArgumentOutOfRangeException(nameof(type), type, "Missão diária inexistente no catálogo.");

        private static OreAmount Quartz(int quantity) => new() { Ore = OreType.Quartz, Quantity = quantity };
    }
}
