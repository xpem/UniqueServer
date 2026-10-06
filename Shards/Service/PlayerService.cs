using Microsoft.EntityFrameworkCore;
using Shards.Model.DTO;
using Shards.Model.Res;
using Shards.Repo;
using Shards.Rules;

namespace Shards.Service
{
    public interface IPlayerService
    {
        /// <summary>Estado completo do jogador. Cria o jogador (com a Mina 1 e a Missão 1) no primeiro acesso.</summary>
        Task<ShardsStateRes> GetStateAsync(int uid);
    }

    /// <summary>
    /// Os serviços do Shards usam o IDbContextFactory direto, e não repositórios por entidade como os outros módulos:
    /// cada comando do jogo grava Player, Mine, Inventory e Mission no mesmo SaveChanges (atomicidade e xmin).
    /// </summary>
    public class PlayerService(IDbContextFactory<ShardsDbctx> dbFactory, GameRules rules, ShardsMapper mapper) : IPlayerService
    {
        public Task<ShardsStateRes> GetStateAsync(int uid) => ConcurrencyRetry.RunAsync(async () =>
        {
            await using ShardsDbctx ctx = await dbFactory.CreateDbContextAsync();

            PlayerDTO player = await FindOrCreatePlayerAsync(ctx, uid);

            // persiste a energia regenerada; com a energia já cheia não há o que gravar
            EnergyState energy = rules.RegenerateEnergy(player.Energy, player.MaxEnergy, player.LastEnergyUpdateUtc);
            if (energy.Energy != player.Energy)
            {
                player.Energy = energy.Energy;
                player.LastEnergyUpdateUtc = energy.LastUpdateUtc;
                player.UpdatedAt = rules.UtcNow;
                await ctx.SaveChangesAsync();
            }

            var skills = await ctx.PlayerSkill.AsNoTracking().Where(s => s.PlayerId == player.Id).ToListAsync();
            var inventory = await ctx.InventoryItem.AsNoTracking().Where(i => i.PlayerId == player.Id).ToListAsync();
            var mines = await ctx.Mine.AsNoTracking().Where(m => m.PlayerId == player.Id).ToListAsync();
            var missions = await ctx.PlayerMission.AsNoTracking().Where(m => m.PlayerId == player.Id).ToListAsync();

            return mapper.ToStateRes(player, skills, inventory, mines, missions);
        });

        private async Task<PlayerDTO> FindOrCreatePlayerAsync(ShardsDbctx ctx, int uid)
        {
            PlayerDTO? player = await ctx.Player.FirstOrDefaultAsync(p => p.UserId == uid);
            if (player is not null) return player;

            try
            {
                return await CreatePlayerAsync(ctx, uid);
            }
            catch (DbUpdateException ex) when (ConcurrencyRetry.IsUniqueViolation(ex))
            {
                // duas requisições do primeiro acesso ao mesmo tempo: a outra criou primeiro (índice único em UserId)
                ctx.ChangeTracker.Clear();

                return await ctx.Player.FirstOrDefaultAsync(p => p.UserId == uid) ?? throw new InvalidOperationException(
                    $"Falha ao criar o jogador do usuário {uid}.", ex);
            }
        }

        /// <summary>Cria Player + Mina 1 + Missão 1 numa transação.</summary>
        private async Task<PlayerDTO> CreatePlayerAsync(ShardsDbctx ctx, int uid)
        {
            DateTime now = rules.UtcNow;
            Config.MineOptions firstMine = rules.GetMine(1);

            PlayerDTO? created = null;

            await ctx.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                ctx.ChangeTracker.Clear();
                await using var tx = await ctx.Database.BeginTransactionAsync();

                var player = new PlayerDTO
                {
                    UserId = uid,
                    CreatedAt = now,
                    UpdatedAt = now,
                    Energy = rules.Balance.Energy.MaxEnergy,
                    MaxEnergy = rules.Balance.Energy.MaxEnergy,
                    LastEnergyUpdateUtc = now,
                };
                ctx.Player.Add(player);
                await ctx.SaveChangesAsync();

                ctx.Mine.Add(new MineDTO
                {
                    PlayerId = player.Id,
                    MineNumber = firstMine.MineNumber,
                    CreatedAt = now,
                    CartCapacity = firstMine.CartCapacity,
                    OresPerMinute = firstMine.OresPerMinute,
                    LastCartCollectionUtc = now,
                });
                ctx.PlayerMission.Add(new PlayerMissionDTO
                {
                    PlayerId = player.Id,
                    MissionType = MissionCatalog.First.Type,
                    Status = MissionStatus.Active,
                });
                await ctx.SaveChangesAsync();

                await tx.CommitAsync();
                created = player;
            });

            return created!;
        }
    }
}
