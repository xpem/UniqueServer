using Microsoft.EntityFrameworkCore;
using Shards.Config;
using Shards.Errors;
using Shards.Model.DTO;
using Shards.Model.Res;
using Shards.Repo;
using Shards.Rules;

namespace Shards.Service
{
    public interface IMineService
    {
        /// <summary>Mineração ativa: gasta energia, sorteia drops e dá XP, N vezes numa só transação.</summary>
        Task<MineRes> MineAsync(int uid, int mineNumber, int times);

        /// <summary>Esvazia o vagonete: passa os minérios acumulados para o inventário e zera o relógio dele.</summary>
        Task<CollectCartRes> CollectCartAsync(int uid, int mineNumber);

        /// <summary>Compra a próxima mina configurada, debitando o custo do inventário.</summary>
        Task<UnlockMineRes> UnlockNextAsync(int uid);
    }

    public class MineService(IDbContextFactory<ShardsDbctx> dbFactory, GameRules rules, ShardsMapper mapper) : IMineService
    {
        public Task<MineRes> MineAsync(int uid, int mineNumber, int times)
        {
            EnergyOptions energyConfig = rules.Balance.Energy;

            if (times < 1 || times > energyConfig.MaxMinesPerRequest)
                throw new ShardsException(ShardsErrorCode.InvalidTimes, $"times deve ficar entre 1 e {energyConfig.MaxMinesPerRequest}.");

            return ConcurrencyRetry.RunAsync(async () =>
            {
                await using ShardsDbctx ctx = await dbFactory.CreateDbContextAsync();
                DateTime now = rules.UtcNow;

                PlayerDTO player = await ShardsQueries.GetPlayerAsync(ctx, uid);
                MineDTO mine = await ShardsQueries.GetMineAsync(ctx, player.Id, mineNumber);
                MineOptions mineConfig = rules.GetMine(mineNumber);

                // energia: regenera e debita; o resto parcial do relógio é preservado
                EnergyState energy = rules.RegenerateEnergy(player.Energy, player.MaxEnergy, player.LastEnergyUpdateUtc);
                int cost = times * energyConfig.CostPerMine;
                if (energy.Energy < cost) throw new ShardsException(ShardsErrorCode.NotEnoughEnergy);

                player.Energy = energy.Energy - cost;
                player.LastEnergyUpdateUtc = energy.LastUpdateUtc;

                // drops: 1 sorteio por mineração; a skill DoubleDrop pode render um minério extra no mesmo custo
                int doubleDropLevel = await ShardsQueries.GetSkillLevelAsync(ctx, player.Id, SkillType.DoubleDrop);
                SortedDictionary<OreType, int> drops = [];
                int doubleDrops = 0, xpGained = 0;

                for (int i = 0; i < times; i++)
                {
                    DropOptions drop = rules.RollDrop(mineConfig);
                    int quantity = 1;

                    if (rules.RollDoubleDrop(doubleDropLevel))
                    {
                        quantity = 2;
                        doubleDrops++;
                    }

                    drops[drop.Ore] = drops.GetValueOrDefault(drop.Ore) + quantity;
                    xpGained += drop.Xp;
                }

                List<InventoryItemDTO> inventory = await ShardsQueries.GetInventoryAsync(ctx, player.Id);
                foreach ((OreType ore, int quantity) in drops)
                    InventoryItemService.Add(ctx, inventory, player.Id, ore, quantity);

                // experiência: cada nível ganho dá 1 ponto de habilidade
                XpResult xp = rules.AddExperience(player.Level, player.Experience, xpGained);
                player.Level = xp.Level;
                player.Experience = xp.Experience;
                player.SkillPoints += xp.LevelsGained;
                player.UpdatedAt = now;

                // Missão 1 conta a energia gasta
                List<PlayerMissionDTO> missions = await ShardsQueries.GetMissionsAsync(ctx, player.Id);
                List<PlayerMissionDTO> changedMissions = MissionTracker.AdvanceChanged(missions, MissionType.FirstPickaxe, cost, now);

                await ctx.SaveChangesAsync();

                return new MineRes
                {
                    Drops = drops.Select(d => new OreAmountRes { OreType = d.Key, Quantity = d.Value }).ToList(),
                    DoubleDrops = doubleDrops,
                    XpGained = xpGained,
                    LevelUp = xp.LevelsGained > 0,
                    Player = mapper.ToPlayerRes(player),
                    Missions = changedMissions.Select(mapper.ToMissionRes).ToList(),
                };
            });
        }

        public Task<UnlockMineRes> UnlockNextAsync(int uid) => ConcurrencyRetry.RunAsync(async () =>
        {
            await using ShardsDbctx ctx = await dbFactory.CreateDbContextAsync();
            DateTime now = rules.UtcNow;

            PlayerDTO player = await ShardsQueries.GetPlayerAsync(ctx, uid);
            List<MineDTO> mines = await ctx.Mine.Where(m => m.PlayerId == player.Id).ToListAsync();

            MineOptions next = rules.GetNextMine(mines.Select(m => m.MineNumber).DefaultIfEmpty(0).Max())
                ?? throw new ShardsException(ShardsErrorCode.NoMoreMines);

            // custo: Remove falha com NotEnoughResources, sem alterar nada, se faltar qualquer minério
            List<InventoryItemDTO> inventory = await ShardsQueries.GetInventoryAsync(ctx, player.Id);
            InventoryItemService.Remove(inventory, next.UnlockCost.Select(c => (c.Ore, c.Quantity)).ToList());

            var mine = new MineDTO
            {
                PlayerId = player.Id,
                MineNumber = next.MineNumber,
                CreatedAt = now,
                CartCapacity = next.CartCapacity,
                OresPerMinute = next.OresPerMinute,
                LastCartCollectionUtc = now,
            };
            ctx.Mine.Add(mine);

            // Missão 3: desbloquear a segunda mina
            List<PlayerMissionDTO> missions = await ShardsQueries.GetMissionsAsync(ctx, player.Id);
            List<PlayerMissionDTO> changedMissions = MissionTracker.AdvanceChanged(missions, MissionType.OperationalExpansion, 1, now);

            await ctx.SaveChangesAsync();

            return new UnlockMineRes
            {
                Mine = mapper.ToMineStateRes(mine),
                Inventory = mapper.ToInventoryRes(inventory),
                Missions = changedMissions.Select(mapper.ToMissionRes).ToList(),
            };
        });

        public Task<CollectCartRes> CollectCartAsync(int uid, int mineNumber) => ConcurrencyRetry.RunAsync(async () =>
        {
            await using ShardsDbctx ctx = await dbFactory.CreateDbContextAsync();
            DateTime now = rules.UtcNow;

            PlayerDTO player = await ShardsQueries.GetPlayerAsync(ctx, uid);
            MineDTO mine = await ShardsQueries.GetMineAsync(ctx, player.Id, mineNumber);
            MineOptions mineConfig = rules.GetMine(mineNumber);

            int amount = rules.CartAmount(mine.CartCapacity, mine.OresPerMinute, mine.LastCartCollectionUtc);
            if (amount <= 0) throw new ShardsException(ShardsErrorCode.CartEmpty);

            // cada minério do vagonete sorteia a tabela da mina; a coleta não dá XP nem drop duplo
            SortedDictionary<OreType, int> collected = [];
            for (int i = 0; i < amount; i++)
            {
                OreType ore = rules.RollDrop(mineConfig).Ore;
                collected[ore] = collected.GetValueOrDefault(ore) + 1;
            }

            List<InventoryItemDTO> inventory = await ShardsQueries.GetInventoryAsync(ctx, player.Id);
            foreach ((OreType ore, int quantity) in collected)
                InventoryItemService.Add(ctx, inventory, player.Id, ore, quantity);

            mine.LastCartCollectionUtc = now;

            await ctx.SaveChangesAsync();

            return new CollectCartRes
            {
                Collected = collected.Select(c => new OreAmountRes { OreType = c.Key, Quantity = c.Value }).ToList(),
                Total = amount,
                Cart = mapper.ToMineStateRes(mine),
                Inventory = mapper.ToInventoryRes(inventory),
            };
        });
    }
}
