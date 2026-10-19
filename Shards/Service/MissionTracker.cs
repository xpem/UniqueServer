using Shards.Model.DTO;
using Shards.Rules;

namespace Shards.Service
{
    /// <summary>Avança o progresso das missões do jogador (entidades já rastreadas pelo DbContext, sem acesso a banco).</summary>
    public static class MissionTracker
    {
        /// <summary>
        /// Soma progresso à missão ativa do tipo informado e a marca como Completed ao atingir a meta.
        /// Devolve a missão alterada, ou null se ela não existe ou não está ativa (já concluída/resgatada, ou ainda não liberada).
        /// </summary>
        public static PlayerMissionDTO? Advance(IEnumerable<PlayerMissionDTO> missions, MissionType type, int amount, DateTime nowUtc)
        {
            PlayerMissionDTO? mission = missions.FirstOrDefault(m => m.MissionType == type && m.Status == MissionStatus.Active);
            if (mission is null) return null;

            int goal = MissionCatalog.Get(type).Goal;
            mission.Progress = Math.Min(goal, mission.Progress + amount);

            if (mission.Progress >= goal)
            {
                mission.Status = MissionStatus.Completed;
                mission.CompletedAt = nowUtc;
            }

            return mission;
        }

        /// <summary>Como <see cref="Advance"/>, mas devolve a lista das missões alteradas (vazia ou com 1), pronta para a resposta.</summary>
        public static List<PlayerMissionDTO> AdvanceChanged(IEnumerable<PlayerMissionDTO> missions, MissionType type, int amount, DateTime nowUtc) =>
            Advance(missions, type, amount, nowUtc) is { } changed ? [changed] : [];

        /// <summary>
        /// Cria as missões que faltam na cadeia: para cada missão já resgatada cuja seguinte ainda não existe, cria a
        /// seguinte como ativa (ou já concluída, se o jogador já fez o que ela pede). Devolve só as criadas.
        /// Serve ao resgate e ao estado, para missões novas do catálogo chegarem a quem já resgatou a anterior.
        /// </summary>
        public static List<PlayerMissionDTO> CreateMissingNext(IReadOnlyCollection<PlayerMissionDTO> missions, int playerId, MissionFacts facts, DateTime nowUtc)
        {
            List<PlayerMissionDTO> created = [];

            for (int i = 0; i < MissionCatalog.All.Count - 1; i++)
            {
                MissionType current = MissionCatalog.All[i].Type;
                MissionType next = MissionCatalog.All[i + 1].Type;

                bool claimed = missions.Any(m => m.MissionType == current && m.Status == MissionStatus.Claimed);
                bool exists = missions.Any(m => m.MissionType == next) || created.Any(m => m.MissionType == next);
                if (!claimed || exists) continue;

                var mission = new PlayerMissionDTO { PlayerId = playerId, MissionType = next, Status = MissionStatus.Active };
                CompleteIfAlreadySatisfied(mission, facts, nowUtc);
                created.Add(mission);
            }

            return created;
        }

        /// <summary>
        /// Uma missão só é liberada quando a anterior é resgatada. Se o jogador já fez o que ela pede antes disso
        /// (ex.: distribuiu o ponto de habilidade ou comprou a mina 2 antes de resgatar a missão anterior),
        /// ela já nasce concluída, em vez de exigir repetir a ação.
        /// </summary>
        public static void CompleteIfAlreadySatisfied(PlayerMissionDTO mission, MissionFacts facts, DateTime nowUtc)
        {
            if (!IsSatisfied(mission.MissionType, facts)) return;

            mission.Progress = MissionCatalog.Get(mission.MissionType).Goal;
            mission.Status = MissionStatus.Completed;
            mission.CompletedAt = nowUtc;
        }

        /// <summary>
        /// Conclui as missões ATIVAS dos tipos informados cuja condição já vale. Cada serviço passa só os tipos que a ação dele
        /// pode ter cumprido, e só os fatos que esses tipos leem. Devolve as missões alteradas.
        /// </summary>
        public static List<PlayerMissionDTO> CompleteSatisfiedChanged(IEnumerable<PlayerMissionDTO> missions, IReadOnlyCollection<MissionType> types, MissionFacts facts, DateTime nowUtc)
        {
            List<PlayerMissionDTO> changed = [];

            foreach (PlayerMissionDTO mission in missions.Where(m => m.Status == MissionStatus.Active && types.Contains(m.MissionType)))
            {
                CompleteIfAlreadySatisfied(mission, facts, nowUtc);
                if (mission.Status == MissionStatus.Completed) changed.Add(mission);
            }

            return changed;
        }

        private static readonly OreType[] Gems = [OreType.Ruby, OreType.Sapphire, OreType.Emerald];

        /// <summary>
        /// Missões que dependem de uma condição do estado do jogador (as de contagem, como PolishBatch, só avançam por ação).
        /// Estágios usam a EXISTÊNCIA da linha do inventário, não a quantidade: a linha nasce no primeiro item e não some,
        /// então gastar ou vender o item depois não desfaz a missão.
        /// </summary>
        public static bool IsSatisfied(MissionType type, MissionFacts facts)
        {
            IReadOnlyCollection<PlayerSkillDTO> skills = facts.Skills ?? [];
            IReadOnlyCollection<MineDTO> mines = facts.Mines ?? [];
            IReadOnlyCollection<InventoryItemDTO> inventory = facts.Inventory ?? [];

            bool Has(OreType ore, ItemStage stage) => inventory.Any(i => i.OreType == ore && i.Stage == stage);
            int SkillLevel(SkillType skill) => skills.Where(s => s.SkillType == skill).Select(s => s.Level).FirstOrDefault();

            return type switch
            {
                MissionType.Specialization => skills.Any(s => s.Level > 0),
                MissionType.OperationalExpansion => mines.Any(m => m.MineNumber >= 2),
                MissionType.FirstGem => mines.Any(m => m.ProductionType != OreType.Quartz),
                MissionType.CartTraining => SkillLevel(SkillType.CartCapacity) > 0,
                MissionType.CartUpgrade => mines.Any(m => m.CartLevel > 0),
                MissionType.OpenWorkshop => SkillLevel(SkillType.Workshop) > 0,
                MissionType.FirstPolish => inventory.Any(i => i.Stage is ItemStage.Polished or ItemStage.Faceted),
                MissionType.FirstFacet => inventory.Any(i => i.Stage == ItemStage.Faceted),
                MissionType.Diversification => mines.Select(m => m.ProductionType).Where(p => p != OreType.Quartz).Distinct().Count() >= 2,
                MissionType.Collector => Gems.All(gem => inventory.Any(i => i.OreType == gem)),
                MissionType.FullSet => Gems.All(gem => Has(gem, ItemStage.Faceted)),
                MissionType.ThirdMine => mines.Count >= 3,
                MissionType.DoubleLuck => SkillLevel(SkillType.DoubleDrop) >= 3,
                _ => false,
            };
        }
    }

    /// <summary>O que as condições de missão leem do jogador. Cada chamador preenche só o que os tipos avaliados precisam.</summary>
    public record MissionFacts(
        IReadOnlyCollection<PlayerSkillDTO>? Skills = null,
        IReadOnlyCollection<MineDTO>? Mines = null,
        IReadOnlyCollection<InventoryItemDTO>? Inventory = null);
}
