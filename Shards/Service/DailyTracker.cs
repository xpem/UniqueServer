using Shards.Model.DTO;
using Shards.Rules;

namespace Shards.Service
{
    /// <summary>O que decide quais diárias o jogador pode receber.</summary>
    public record DailyFacts(bool HasGemMine, int WorkshopLevel);

    /// <summary>Cria e avança as missões diárias (entidades já rastreadas pelo DbContext, sem acesso a banco).</summary>
    public static class DailyTracker
    {
        /// <summary>As diárias só existem depois que o jogador resgatou a Missão 1, para não competirem com o início do tutorial.</summary>
        public static bool IsUnlocked(IEnumerable<PlayerMissionDTO> missions) =>
            missions.Any(m => m.MissionType == MissionType.FirstPickaxe && m.Status == MissionStatus.Claimed);

        public static bool IsEligible(DailyMissionType type, DailyFacts facts) => type switch
        {
            DailyMissionType.Gems => facts.HasGemMine,
            DailyMissionType.Polish => facts.WorkshopLevel >= 1,
            DailyMissionType.Facet => facts.WorkshopLevel >= 2,
            _ => true,
        };

        /// <summary>
        /// Cria as diárias do dia, se ainda não existem: Login (já concluída) mais (perDay - 1) sorteadas entre as elegíveis.
        /// O sorteio é determinístico por (jogador, dia): o mesmo jogador vê as mesmas tarefas a cada leitura do dia.
        /// </summary>
        public static List<DailyMissionDTO> CreateMissingToday(IReadOnlyCollection<DailyMissionDTO> today, int playerId, DateOnly day, int perDay, DailyFacts facts, DateTime nowUtc)
        {
            if (today.Count > 0) return [];

            List<DailyMissionType> picked = [DailyMissionType.Login];

            List<DailyMissionType> pool = DailyMissionCatalog.All
                .Select(d => d.Type)
                .Where(t => t != DailyMissionType.Login && IsEligible(t, facts))
                .OrderBy(t => (int)t)
                .ToList();

            // Fisher-Yates com semente fixa (nunca HashCode.Combine/GetHashCode: variam a cada execução)
            var random = new Random(unchecked(playerId * 7919 + day.DayNumber * 104729));
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            picked.AddRange(pool.Take(Math.Max(0, perDay - 1)));

            List<DailyMissionDTO> created = [];
            foreach (DailyMissionType type in picked)
            {
                var mission = new DailyMissionDTO { PlayerId = playerId, Day = day, DailyType = type };

                if (type == DailyMissionType.Login)
                {
                    mission.Progress = DailyMissionCatalog.Get(type).Goal;
                    mission.Status = MissionStatus.Completed;
                    mission.CompletedAt = nowUtc;
                }

                created.Add(mission);
            }

            return created;
        }

        /// <summary>Soma progresso às diárias ATIVAS dos tipos informados e conclui as que chegam à meta. Devolve só as alteradas.</summary>
        public static List<DailyMissionDTO> AdvanceChanged(IEnumerable<DailyMissionDTO> today, IEnumerable<(DailyMissionType Type, int Amount)> progress, DateTime nowUtc)
        {
            List<DailyMissionDTO> changed = [];
            DailyMissionDTO[] missions = today.ToArray();

            foreach ((DailyMissionType type, int amount) in progress)
            {
                if (amount <= 0) continue;

                DailyMissionDTO? mission = missions.FirstOrDefault(m => m.DailyType == type && m.Status == MissionStatus.Active);
                if (mission is null) continue;

                int goal = DailyMissionCatalog.Get(type).Goal;
                mission.Progress = Math.Min(goal, mission.Progress + amount);

                if (mission.Progress >= goal)
                {
                    mission.Status = MissionStatus.Completed;
                    mission.CompletedAt = nowUtc;
                }

                if (!changed.Contains(mission)) changed.Add(mission);
            }

            return changed;
        }

        public static List<DailyMissionDTO> AdvanceChanged(IEnumerable<DailyMissionDTO> today, DailyMissionType type, int amount, DateTime nowUtc) =>
            AdvanceChanged(today, [(type, amount)], nowUtc);

        /// <summary>Progresso de minério de uma ação: quartzo e gemas brutas obtidas (para Quartz e Gems).</summary>
        public static IEnumerable<(DailyMissionType, int)> OreProgress(IEnumerable<KeyValuePair<OreType, int>> ores)
        {
            KeyValuePair<OreType, int>[] list = ores.ToArray();
            yield return (DailyMissionType.Quartz, list.Where(o => o.Key == OreType.Quartz).Sum(o => o.Value));
            yield return (DailyMissionType.Gems, list.Where(o => o.Key != OreType.Quartz).Sum(o => o.Value));
        }
    }
}
