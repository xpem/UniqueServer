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

        /// <summary>
        /// Muda o tipo de produção da mina (ex.: "Sapphire"), pagando o custo da nova produção em minério.
        /// Voltar para Quartz é de graça. O vagonete é esvaziado antes, com a produção antiga.
        /// </summary>
        Task<SetProductionRes> SetProductionAsync(int uid, int mineNumber, string? focus);

        /// <summary>
        /// Sobe o vagonete da mina para o próximo nível, pagando o custo em minério. Exige a skill CartCapacity no nível (teto).
        /// O vagonete é esvaziado antes, para a capacidade nova não valer retroativamente sobre o tempo em que ele já estava cheio.
        /// </summary>
        Task<UpgradeCartRes> UpgradeCartAsync(int uid, int mineNumber);
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
                ProductionOptions production = rules.GetProduction(mine.ProductionType);

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
                    DropOptions drop = rules.RollDrop(production);
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

            // espaço (skill MineSlots) e quartzo são requisitos separados: sem espaço livre, nem chega a cobrar
            int slotsLevel = await ShardsQueries.GetSkillLevelAsync(ctx, player.Id, SkillType.MineSlots);
            if (mines.Count >= rules.MineSlotsFor(slotsLevel)) throw new ShardsException(ShardsErrorCode.MineSlotsFull);

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
                ProductionType = OreType.Quartz,
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

            int amount = rules.CartAmount(mine.CartCapacity, mine.OresPerMinute, mine.LastCartCollectionUtc);
            if (amount <= 0) throw new ShardsException(ShardsErrorCode.CartEmpty);

            List<InventoryItemDTO> inventory = await ShardsQueries.GetInventoryAsync(ctx, player.Id);
            SortedDictionary<OreType, int> collected = EmptyCart(ctx, mine, player.Id, inventory, now);

            await ctx.SaveChangesAsync();

            return new CollectCartRes
            {
                Collected = collected.Select(c => new OreAmountRes { OreType = c.Key, Quantity = c.Value }).ToList(),
                Total = amount,
                Cart = mapper.ToMineStateRes(mine),
                Inventory = mapper.ToInventoryRes(inventory),
            };
        });

        public Task<SetProductionRes> SetProductionAsync(int uid, int mineNumber, string? focus)
        {
            OreType target = ShardsErrors.ParseProduction(focus);
            ProductionOptions targetProduction = rules.FindProduction(target) ?? throw new ShardsException(ShardsErrorCode.InvalidProduction);

            return ConcurrencyRetry.RunAsync(async () =>
            {
                await using ShardsDbctx ctx = await dbFactory.CreateDbContextAsync();
                DateTime now = rules.UtcNow;

                PlayerDTO player = await ShardsQueries.GetPlayerAsync(ctx, uid);
                MineDTO mine = await ShardsQueries.GetMineAsync(ctx, player.Id, mineNumber);

                if (mine.ProductionType == target) throw new ShardsException(ShardsErrorCode.SameProduction);

                List<InventoryItemDTO> inventory = await ShardsQueries.GetInventoryAsync(ctx, player.Id);

                // 1) esvazia o vagonete com a produção ANTIGA: o tempo acumulado foi trabalhado nela
                SortedDictionary<OreType, int> collected = EmptyCart(ctx, mine, player.Id, inventory, now);

                // 2) paga a nova produção. O que acabou de sair do vagonete já conta para pagar; sem recursos
                //    suficientes lançamos antes do SaveChanges e nada, nem a coleta, é gravado
                List<(OreType Ore, int Quantity)> cost = targetProduction.SwitchCost.Select(c => (c.Ore, c.Quantity)).ToList();
                InventoryItemService.Remove(inventory, cost);

                mine.ProductionType = target;

                // Missão 4: pôr uma mina para produzir uma gema (voltar ao quartzo não conta)
                List<PlayerMissionDTO> changedMissions = [];
                if (target != OreType.Quartz)
                {
                    List<PlayerMissionDTO> missions = await ShardsQueries.GetMissionsAsync(ctx, player.Id);
                    changedMissions = MissionTracker.AdvanceChanged(missions, MissionType.FirstGem, 1, now);
                }

                await ctx.SaveChangesAsync();

                return new SetProductionRes
                {
                    Mine = mapper.ToMineStateRes(mine),
                    Collected = collected.Select(c => new OreAmountRes { OreType = c.Key, Quantity = c.Value }).ToList(),
                    Spent = cost.Select(c => new OreAmountRes { OreType = c.Ore, Quantity = c.Quantity }).ToList(),
                    Inventory = mapper.ToInventoryRes(inventory),
                    Missions = changedMissions.Select(mapper.ToMissionRes).ToList(),
                };
            });
        }

        public Task<UpgradeCartRes> UpgradeCartAsync(int uid, int mineNumber) => ConcurrencyRetry.RunAsync(async () =>
        {
            await using ShardsDbctx ctx = await dbFactory.CreateDbContextAsync();
            DateTime now = rules.UtcNow;

            PlayerDTO player = await ShardsQueries.GetPlayerAsync(ctx, uid);
            MineDTO mine = await ShardsQueries.GetMineAsync(ctx, player.Id, mineNumber);

            CartUpgradeOptions next = rules.NextCartUpgrade(mine.CartLevel) ?? throw new ShardsException(ShardsErrorCode.CartMaxLevel);

            // a skill é o teto do nível de qualquer mina; sem permissão, nem chega a cobrar
            int skillLevel = await ShardsQueries.GetSkillLevelAsync(ctx, player.Id, SkillType.CartCapacity);
            if (next.Level > skillLevel) throw new ShardsException(ShardsErrorCode.CartUpgradeLocked);

            List<InventoryItemDTO> inventory = await ShardsQueries.GetInventoryAsync(ctx, player.Id);

            // esvazia antes: sem isso, a capacidade nova contaria o tempo em que o vagonete já estava cheio (produção parada)
            SortedDictionary<OreType, int> collected = EmptyCart(ctx, mine, player.Id, inventory, now);

            // o que saiu do vagonete já conta para pagar; sem recursos, lançamos antes do SaveChanges e nada é gravado
            List<(OreType Ore, int Quantity)> cost = next.Cost.Select(c => (c.Ore, c.Quantity)).ToList();
            InventoryItemService.Remove(inventory, cost);

            mine.CartLevel = next.Level;
            mine.CartCapacity = next.Capacity;

            await ctx.SaveChangesAsync();

            return new UpgradeCartRes
            {
                Mine = mapper.ToMineStateRes(mine),
                Collected = collected.Select(c => new OreAmountRes { OreType = c.Key, Quantity = c.Value }).ToList(),
                Spent = cost.Select(c => new OreAmountRes { OreType = c.Ore, Quantity = c.Quantity }).ToList(),
                Inventory = mapper.ToInventoryRes(inventory),
            };
        });

        /// <summary>
        /// Passa os minérios acumulados no vagonete para o inventário (sorteando a tabela da produção atual da mina,
        /// sem XP nem drop duplo) e zera o relógio dele. Se não há nada acumulado, não altera nada.
        /// </summary>
        private SortedDictionary<OreType, int> EmptyCart(ShardsDbctx ctx, MineDTO mine, int playerId, List<InventoryItemDTO> inventory, DateTime now)
        {
            SortedDictionary<OreType, int> collected = [];

            int amount = rules.CartAmount(mine.CartCapacity, mine.OresPerMinute, mine.LastCartCollectionUtc);
            if (amount <= 0) return collected;

            ProductionOptions production = rules.GetProduction(mine.ProductionType);
            for (int i = 0; i < amount; i++)
            {
                OreType ore = rules.RollDrop(production).Ore;
                collected[ore] = collected.GetValueOrDefault(ore) + 1;
            }

            foreach ((OreType ore, int quantity) in collected)
                InventoryItemService.Add(ctx, inventory, playerId, ore, quantity);

            mine.LastCartCollectionUtc = now;

            return collected;
        }
    }
}
