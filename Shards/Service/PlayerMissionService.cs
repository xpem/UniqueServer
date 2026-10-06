using Microsoft.EntityFrameworkCore;
using Shards.Errors;
using Shards.Model.DTO;
using Shards.Model.Res;
using Shards.Repo;
using Shards.Rules;

namespace Shards.Service
{
    public interface IPlayerMissionService
    {
        /// <summary>Resgata a recompensa de uma missão concluída e libera a próxima da cadeia. missionType é o nome da missão.</summary>
        Task<ClaimMissionRes> ClaimAsync(int uid, string? missionType);
    }

    public class PlayerMissionService(IDbContextFactory<ShardsDbctx> dbFactory, GameRules rules, ShardsMapper mapper) : IPlayerMissionService
    {
        public Task<ClaimMissionRes> ClaimAsync(int uid, string? missionType)
        {
            MissionType type = ShardsErrors.ParseMissionType(missionType);

            return ConcurrencyRetry.RunAsync(async () =>
            {
                await using ShardsDbctx ctx = await dbFactory.CreateDbContextAsync();
                DateTime now = rules.UtcNow;

                PlayerDTO player = await ShardsQueries.GetPlayerAsync(ctx, uid);
                List<PlayerMissionDTO> missions = await ShardsQueries.GetMissionsAsync(ctx, player.Id);

                PlayerMissionDTO mission = missions.FirstOrDefault(m => m.MissionType == type)
                    ?? throw new ShardsException(ShardsErrorCode.MissionNotFound);

                if (mission.Status == MissionStatus.Claimed) throw new ShardsException(ShardsErrorCode.MissionAlreadyClaimed);
                if (mission.Status != MissionStatus.Completed) throw new ShardsException(ShardsErrorCode.MissionNotCompleted);

                // recompensa: o token xmin da missão garante que dois resgates simultâneos paguem uma vez só
                MissionDefinition definition = MissionCatalog.Get(type);

                XpResult xp = rules.AddExperience(player.Level, player.Experience, definition.Reward.Xp);
                player.Level = xp.Level;
                player.Experience = xp.Experience;
                player.SkillPoints += xp.LevelsGained;
                player.UpdatedAt = now;

                List<InventoryItemDTO> inventory = await ShardsQueries.GetInventoryAsync(ctx, player.Id);
                foreach (var ore in definition.Reward.Ores)
                    InventoryItemService.Add(ctx, inventory, player.Id, ore.Ore, ore.Quantity);

                mission.Status = MissionStatus.Claimed;
                mission.ClaimedAt = now;

                List<PlayerMissionDTO> changedMissions = [mission];

                // libera a próxima missão da cadeia (já concluída, se o jogador já tiver feito o que ela pede)
                MissionDefinition? next = MissionCatalog.Next(type);
                if (next is not null && !missions.Any(m => m.MissionType == next.Type))
                {
                    var nextMission = new PlayerMissionDTO { PlayerId = player.Id, MissionType = next.Type, Status = MissionStatus.Active };

                    List<PlayerSkillDTO> skills = await ctx.PlayerSkill.AsNoTracking().Where(s => s.PlayerId == player.Id).ToListAsync();
                    List<MineDTO> mines = await ctx.Mine.AsNoTracking().Where(m => m.PlayerId == player.Id).ToListAsync();
                    MissionTracker.CompleteIfAlreadySatisfied(nextMission, skills, mines, now);

                    ctx.PlayerMission.Add(nextMission);
                    changedMissions.Add(nextMission);
                }

                await ctx.SaveChangesAsync();

                return new ClaimMissionRes
                {
                    Rewards = mapper.ToMissionRes(mission).Reward,
                    LevelUp = xp.LevelsGained > 0,
                    Player = mapper.ToPlayerRes(player),
                    Inventory = mapper.ToInventoryRes(inventory),
                    Missions = changedMissions.Select(mapper.ToMissionRes).ToList(),
                };
            });
        }
    }
}
