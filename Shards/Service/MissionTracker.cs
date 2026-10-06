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
        /// Uma missão só é liberada quando a anterior é resgatada. Se o jogador já fez o que ela pede antes disso
        /// (ex.: distribuiu o ponto de habilidade ou comprou a mina 2 antes de resgatar a missão anterior),
        /// ela já nasce concluída, em vez de exigir repetir a ação.
        /// </summary>
        public static void CompleteIfAlreadySatisfied(PlayerMissionDTO mission, IEnumerable<PlayerSkillDTO> skills, IEnumerable<MineDTO> mines, DateTime nowUtc)
        {
            bool satisfied = mission.MissionType switch
            {
                MissionType.Specialization => skills.Any(s => s.Level > 0),
                MissionType.OperationalExpansion => mines.Any(m => m.MineNumber >= 2),
                _ => false,
            };

            if (!satisfied) return;

            mission.Progress = MissionCatalog.Get(mission.MissionType).Goal;
            mission.Status = MissionStatus.Completed;
            mission.CompletedAt = nowUtc;
        }
    }
}
