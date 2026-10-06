using Microsoft.EntityFrameworkCore;
using Shards.Errors;
using Shards.Model.DTO;
using Shards.Model.Res;
using Shards.Repo;
using Shards.Rules;

namespace Shards.Service
{
    public interface IPlayerSkillService
    {
        /// <summary>Gasta 1 ponto de habilidade para subir a skill 1 nível. skillType é o nome da skill (ex.: "DoubleDrop").</summary>
        Task<DistributeSkillRes> DistributeAsync(int uid, string? skillType);
    }

    public class PlayerSkillService(IDbContextFactory<ShardsDbctx> dbFactory, GameRules rules, ShardsMapper mapper) : IPlayerSkillService
    {
        public Task<DistributeSkillRes> DistributeAsync(int uid, string? skillType)
        {
            SkillType type = ShardsErrors.ParseSkillType(skillType);

            return ConcurrencyRetry.RunAsync(async () =>
            {
                await using ShardsDbctx ctx = await dbFactory.CreateDbContextAsync();
                DateTime now = rules.UtcNow;

                PlayerDTO player = await ShardsQueries.GetPlayerAsync(ctx, uid);
                List<PlayerSkillDTO> skills = await ctx.PlayerSkill.Where(s => s.PlayerId == player.Id).ToListAsync();

                PlayerSkillDTO? skill = skills.FirstOrDefault(s => s.SkillType == type);
                if ((skill?.Level ?? 0) >= rules.MaxSkillLevel(type)) throw new ShardsException(ShardsErrorCode.SkillMaxLevel);
                if (player.SkillPoints < 1) throw new ShardsException(ShardsErrorCode.NoSkillPoints);

                if (skill is null)
                {
                    skill = new PlayerSkillDTO { PlayerId = player.Id, SkillType = type };
                    skills.Add(skill);
                    ctx.PlayerSkill.Add(skill);
                }

                skill.Level++;
                player.SkillPoints--;
                player.UpdatedAt = now;

                // Missão 2: o primeiro ponto de habilidade distribuído
                List<PlayerMissionDTO> missions = await ShardsQueries.GetMissionsAsync(ctx, player.Id);
                List<PlayerMissionDTO> changedMissions = MissionTracker.AdvanceChanged(missions, MissionType.Specialization, 1, now);

                await ctx.SaveChangesAsync();

                return new DistributeSkillRes
                {
                    Skills = mapper.ToSkillsRes(skills),
                    SkillPoints = player.SkillPoints,
                    Missions = changedMissions.Select(mapper.ToMissionRes).ToList(),
                };
            });
        }
    }
}
