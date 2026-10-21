using Microsoft.EntityFrameworkCore;
using Shards.Config;
using Shards.Errors;
using Shards.Model.DTO;
using Shards.Model.Res;
using Shards.Repo;
using Shards.Rules;

namespace Shards.Service
{
    public interface IDailyService
    {
        /// <summary>Resgata a recompensa de uma missão diária concluída hoje. type é o nome da missão (ex.: "Energy").</summary>
        Task<DailyClaimRes> ClaimAsync(int uid, string? type);

        /// <summary>Resgata o baú de hoje (todas as diárias resgatadas) e atualiza a sequência de dias.</summary>
        Task<DailyClaimRes> ClaimChestAsync(int uid);
    }

    public class DailyService(IDbContextFactory<ShardsDbctx> dbFactory, GameRules rules, ShardsMapper mapper) : IDailyService
    {
        public Task<DailyClaimRes> ClaimAsync(int uid, string? type)
        {
            DailyMissionType dailyType = ShardsErrors.ParseDailyType(type);

            return ConcurrencyRetry.RunAsync(async () =>
            {
                await using ShardsDbctx ctx = await dbFactory.CreateDbContextAsync();
                DateTime now = rules.UtcNow;
                DateOnly day = rules.DailyDay;

                PlayerDTO player = await ShardsQueries.GetPlayerAsync(ctx, uid);
                List<DailyMissionDTO> today = await ShardsQueries.GetDailyAsync(ctx, player.Id, day);

                DailyMissionDTO mission = today.FirstOrDefault(d => d.DailyType == dailyType) ?? throw new ShardsException(ShardsErrorCode.DailyNotFound);
                if (mission.Status == MissionStatus.Claimed) throw new ShardsException(ShardsErrorCode.DailyAlreadyClaimed);
                if (mission.Status != MissionStatus.Completed) throw new ShardsException(ShardsErrorCode.DailyNotCompleted);

                // o token xmin da missão garante que dois resgates simultâneos paguem uma vez só
                DailyMissionDefinition definition = DailyMissionCatalog.Get(dailyType);
                List<InventoryItemDTO> inventory = await ShardsQueries.GetInventoryAsync(ctx, player.Id);
                XpResult xp = Pay(ctx, player, inventory, definition.Reward.Xp, definition.Reward.Ores, now);

                mission.Status = MissionStatus.Claimed;
                mission.ClaimedAt = now;

                await ctx.SaveChangesAsync();

                return Build(player, inventory, today, day, xp, mapper.ToDailyMissionRes(mission).Reward);
            });
        }

        public Task<DailyClaimRes> ClaimChestAsync(int uid) => ConcurrencyRetry.RunAsync(async () =>
        {
            await using ShardsDbctx ctx = await dbFactory.CreateDbContextAsync();
            DateTime now = rules.UtcNow;
            DateOnly day = rules.DailyDay;

            PlayerDTO player = await ShardsQueries.GetPlayerAsync(ctx, uid);
            if (player.LastChestDay == day) throw new ShardsException(ShardsErrorCode.ChestAlreadyClaimed);

            List<DailyMissionDTO> today = await ShardsQueries.GetDailyAsync(ctx, player.Id, day);
            if (today.Count == 0 || today.Any(d => d.Status != MissionStatus.Claimed)) throw new ShardsException(ShardsErrorCode.ChestNotReady);

            // sequência: continua se o último baú foi ontem; senão recomeça no dia 1
            int streakDay = ShardsMapper.NextChestStreakDay(player, day);
            (int chestXp, int chestQuartz) = rules.ChestReward(streakDay);
            List<OreAmount> ores = [new OreAmount { Ore = OreType.Quartz, Quantity = chestQuartz }];

            List<InventoryItemDTO> inventory = await ShardsQueries.GetInventoryAsync(ctx, player.Id);
            XpResult xp = Pay(ctx, player, inventory, chestXp, ores, now);

            player.DailyStreak = streakDay;
            player.LastChestDay = day;

            await ctx.SaveChangesAsync();

            var rewards = new MissionRewardRes { Xp = chestXp, Ores = [new OreAmountRes { OreType = OreType.Quartz, Quantity = chestQuartz }] };

            return Build(player, inventory, today, day, xp, rewards);
        });

        /// <summary>Paga XP (pode subir de nível e dar ponto de habilidade) e minério bruto.</summary>
        private XpResult Pay(ShardsDbctx ctx, PlayerDTO player, List<InventoryItemDTO> inventory, int xpGained, IEnumerable<OreAmount> ores, DateTime now)
        {
            XpResult xp = rules.AddExperience(player.Level, player.Experience, xpGained);
            player.Level = xp.Level;
            player.Experience = xp.Experience;
            player.SkillPoints += xp.LevelsGained;
            player.UpdatedAt = now;

            foreach (OreAmount ore in ores)
                InventoryItemService.Add(ctx, inventory, player.Id, ore.Ore, ore.Quantity);

            return xp;
        }

        private DailyClaimRes Build(PlayerDTO player, List<InventoryItemDTO> inventory, List<DailyMissionDTO> today, DateOnly day, XpResult xp, MissionRewardRes rewards) => new()
        {
            Rewards = rewards,
            LevelUp = xp.LevelsGained > 0,
            Player = mapper.ToPlayerRes(player),
            Inventory = mapper.ToInventoryRes(inventory),
            Daily = mapper.ToDailyStateRes(player, today, day),
        };
    }
}
