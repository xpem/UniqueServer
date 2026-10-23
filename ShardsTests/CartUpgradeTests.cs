using Microsoft.EntityFrameworkCore;
using Shards.Config;
using Shards.Errors;
using Shards.Model.DTO;

namespace ShardsTests
{
    [TestClass]
    public class CartUpgradeRulesTests : GameRulesTestBase
    {
        [TestMethod]
        public void AppSettings_Table_MatchesThePlan()
        {
            int[] capacities = [150, 220, 320, 450, 600];
            int[] costs = [300, 800, 2000, 5000, 12000];

            Assert.AreEqual(5, Balance.CartUpgrades.Count);
            for (int i = 0; i < capacities.Length; i++)
            {
                var upgrade = Balance.CartUpgrades.Single(u => u.Level == i + 1);
                Assert.AreEqual(capacities[i], upgrade.Capacity, $"capacidade do nível {i + 1}");
                Assert.AreEqual(OreType.Quartz, upgrade.Cost.Single().Ore);
                Assert.AreEqual(costs[i], upgrade.Cost.Single().Quantity, $"custo do nível {i + 1}");
            }
        }

        [TestMethod]
        public void SkillMaxLevel_IsTheHighestConfiguredUpgrade()
        {
            Assert.AreEqual(5, Rules.MaxSkillLevel(SkillType.CartCapacity));

            Balance.CartUpgrades.Clear();
            Assert.AreEqual(0, Rules.MaxSkillLevel(SkillType.CartCapacity), "sem melhorias configuradas a skill não tem níveis");
        }

        [TestMethod]
        public void NextCartUpgrade_FollowsTheCurrentLevel()
        {
            Assert.AreEqual(1, Rules.NextCartUpgrade(0)?.Level);
            Assert.AreEqual(5, Rules.NextCartUpgrade(4)?.Level);
            Assert.IsNull(Rules.NextCartUpgrade(5));
        }

        [TestMethod]
        public void Validator_AcceptsNoUpgrades_ButRejectsBrokenTables()
        {
            var validator = new GameBalanceOptionsValidator();

            Balance.CartUpgrades.Clear();
            Assert.IsTrue(validator.Validate(null, Balance).Succeeded);
        }

        [TestMethod]
        public void Validator_RejectsLevelGaps_NonIncreasingCapacity_AndFreeOrNegativeCosts()
        {
            var validator = new GameBalanceOptionsValidator();

            Balance.CartUpgrades.RemoveAll(u => u.Level == 3);
            Assert.IsTrue(validator.Validate(null, Balance).Failures!.Any(f => f.Contains("sem repetir nem pular")));

            Balance.CartUpgrades.Add(new CartUpgradeOptions { Level = 3, Capacity = 220, Cost = [new OreAmount { Ore = OreType.Quartz, Quantity = 1 }] });
            Assert.IsTrue(validator.Validate(null, Balance).Failures!.Any(f => f.Contains("maior que a do nível anterior")));

            Balance.CartUpgrades.Single(u => u.Level == 3).Capacity = 320;
            Balance.CartUpgrades.Single(u => u.Level == 3).Cost[0].Quantity = 0;
            Assert.IsTrue(validator.Validate(null, Balance).Failures!.Any(f => f.Contains("Cost")));
        }
    }

    [TestClass]
    public class CartUpgradeServiceTests : ShardsServiceTestBase
    {
        private async Task SetSkillAsync(SkillType type, int level, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            var skill = await ctx.PlayerSkill.FirstOrDefaultAsync(s => s.PlayerId == player.Id && s.SkillType == type);
            if (skill is null) ctx.PlayerSkill.Add(new PlayerSkillDTO { PlayerId = player.Id, SkillType = type, Level = level });
            else skill.Level = level;
            await ctx.SaveChangesAsync();
        }

        private async Task<MineDTO> MineRow(int mineNumber = 1)
        {
            await using var ctx = Db.CreateDbContext();
            return await ctx.Mine.AsNoTracking().SingleAsync(m => m.MineNumber == mineNumber);
        }

        [TestMethod]
        public async Task NewMines_StartAtLevelZero_WithTheInitialCapacity()
        {
            await SeedPlayerAsync();

            var state = await Players.GetStateAsync(7);

            Assert.AreEqual(0, state.Mines.Single().CartLevel);
            Assert.AreEqual(100, state.Mines.Single().CartCapacity);
        }

        [TestMethod]
        public async Task WithoutTheSkill_IsCartUpgradeLocked_AndDoesNotCharge()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 300);

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.UpgradeCartAsync(7, 1));

            Assert.AreEqual(ShardsErrorCode.CartUpgradeLocked, ex.Code);
            Assert.AreEqual(300, await InventoryOf(OreType.Quartz));
            Assert.AreEqual(0, (await MineRow()).CartLevel);
        }

        [TestMethod]
        public async Task SkillLevelOne_AndThreeHundredQuartz_RaisesTheCapacityToOneFifty()
        {
            await SeedPlayerAsync();
            await SetSkillAsync(SkillType.CartCapacity, 1);
            await GiveOresAsync(OreType.Quartz, 300);

            var res = await Mines.UpgradeCartAsync(7, 1);

            Assert.AreEqual(1, res.Mine.CartLevel);
            Assert.AreEqual(150, res.Mine.CartCapacity);
            Assert.AreEqual(300, res.Spent.Single().Quantity);
            Assert.AreEqual(0, res.Inventory.Count);
            var row = await MineRow();
            Assert.AreEqual(1, row.CartLevel);
            Assert.AreEqual(150, row.CartCapacity);
        }

        [TestMethod]
        public async Task TheSkillIsACeiling_ForEveryMine_NotAPoolToSpend()
        {
            await SeedPlayerAsync();
            await GrantMineSlotAsync(levels: 1);
            await GiveOresAsync(OreType.Quartz, 200);
            await Mines.UnlockNextAsync(7);
            await SetSkillAsync(SkillType.CartCapacity, 2);
            await GiveOresAsync(OreType.Quartz, 2 * (300 + 800) + 1);

            // as duas minas chegam ao nível 2: o nível da skill vale para cada uma
            foreach (int mineNumber in new[] { 1, 2 })
            {
                await Mines.UpgradeCartAsync(7, mineNumber);
                await Mines.UpgradeCartAsync(7, mineNumber);
            }

            Assert.AreEqual(220, (await MineRow(1)).CartCapacity);
            Assert.AreEqual(220, (await MineRow(2)).CartCapacity);

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.UpgradeCartAsync(7, 1));
            Assert.AreEqual(ShardsErrorCode.CartUpgradeLocked, ex.Code, "o nível 3 exige a skill no nível 3");
            Assert.AreEqual(1, await InventoryOf(OreType.Quartz));
        }

        [TestMethod]
        public async Task EachMineHasItsOwnLevel()
        {
            await SeedPlayerAsync();
            await GrantMineSlotAsync(levels: 1);
            await GiveOresAsync(OreType.Quartz, 200 + 300);
            await Mines.UnlockNextAsync(7);
            await SetSkillAsync(SkillType.CartCapacity, 1);

            await Mines.UpgradeCartAsync(7, 2);

            Assert.AreEqual(1, (await MineRow(2)).CartLevel);
            Assert.AreEqual(0, (await MineRow(1)).CartLevel);
            Assert.AreEqual(100, (await MineRow(1)).CartCapacity);
        }

        [TestMethod]
        public async Task NotEnoughQuartz_ChangesNothing_NotEvenTheCart()
        {
            await SeedPlayerAsync();
            await SetSkillAsync(SkillType.CartCapacity, 1);
            await GiveOresAsync(OreType.Quartz, 100);
            Time.Advance(TimeSpan.FromMinutes(10)); // o vagonete teria 3, mas 103 < 300

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.UpgradeCartAsync(7, 1));

            Assert.AreEqual(ShardsErrorCode.NotEnoughResources, ex.Code);
            var row = await MineRow();
            Assert.AreEqual(0, row.CartLevel);
            Assert.AreEqual(Start, row.LastCartCollectionUtc, "a coleta não é gravada se a melhoria falha");
            Assert.AreEqual(100, await InventoryOf(OreType.Quartz));
        }

        [TestMethod]
        public async Task WhatTheCartDelivers_CanCompleteThePayment()
        {
            await SeedPlayerAsync();
            await SetSkillAsync(SkillType.CartCapacity, 1);
            await GiveOresAsync(OreType.Quartz, 299);
            Time.Advance(TimeSpan.FromMinutes(10)); // +3
            Random.Value = 0.0;

            var res = await Mines.UpgradeCartAsync(7, 1);

            Assert.AreEqual(3, res.Collected.Single().Quantity);
            Assert.AreEqual(2, res.Inventory.Single().Quantity, "299 + 3 - 300");
        }

        [TestMethod]
        public async Task TheCartIsEmptiedFirst_SoTheNewCapacityIsNotRetroactive()
        {
            await SeedPlayerAsync();
            await SetSkillAsync(SkillType.CartCapacity, 1);
            await GiveOresAsync(OreType.Quartz, 300);
            Time.Advance(TimeSpan.FromHours(20)); // o vagonete estava cheio (100) há muito tempo
            Random.Value = 0.0;

            var res = await Mines.UpgradeCartAsync(7, 1);

            Assert.AreEqual(100, res.Collected.Single().Quantity, "só o que cabia na capacidade antiga");
            Assert.AreEqual(0, res.Mine.CartAmount, "o tempo parado não vira minério com a capacidade nova");
            Assert.AreEqual(Time.Now.UtcDateTime, (await MineRow()).LastCartCollectionUtc);

            Time.Advance(TimeSpan.FromMinutes(10));
            Assert.AreEqual(3, (await Players.GetStateAsync(7)).Mines.Single().CartAmount, "a partir daí acumula normalmente");
        }

        [TestMethod]
        public async Task CollectingUsesTheNewCapacity()
        {
            await SeedPlayerAsync();
            await SetSkillAsync(SkillType.CartCapacity, 1);
            await GiveOresAsync(OreType.Quartz, 300);
            await Mines.UpgradeCartAsync(7, 1);
            Time.Advance(TimeSpan.FromHours(20));

            var res = await Mines.CollectCartAsync(7, 1);

            Assert.AreEqual(150, res.Total);
        }

        [TestMethod]
        public async Task TopLevel_IsSixHundred_ThenCartMaxLevel()
        {
            await SeedPlayerAsync();
            await SetSkillAsync(SkillType.CartCapacity, 5);
            await GiveOresAsync(OreType.Quartz, 300 + 800 + 2000 + 5000 + 12000 + 9);

            for (int i = 0; i < 5; i++) await Mines.UpgradeCartAsync(7, 1);

            Assert.AreEqual(600, (await MineRow()).CartCapacity);
            Assert.AreEqual(5, (await MineRow()).CartLevel);
            Assert.AreEqual(9, await InventoryOf(OreType.Quartz));

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.UpgradeCartAsync(7, 1));
            Assert.AreEqual(ShardsErrorCode.CartMaxLevel, ex.Code);
            Assert.AreEqual(9, await InventoryOf(OreType.Quartz));
        }

        [TestMethod]
        public async Task UnknownPlayer_AndMineNotOwned_AreRejected()
        {
            var noPlayer = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.UpgradeCartAsync(99, 1));
            Assert.AreEqual(ShardsErrorCode.PlayerNotFound, noPlayer.Code);

            await SeedPlayerAsync();
            var noMine = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.UpgradeCartAsync(7, 2));
            Assert.AreEqual(ShardsErrorCode.MineNotFound, noMine.Code);
        }

        [TestMethod]
        public async Task State_ShowsTheNextUpgrade_WithCostBalanceAndSkillPermission()
        {
            await SeedPlayerAsync();

            var locked = (await Players.GetStateAsync(7)).CartUpgrades.Single();
            Assert.AreEqual(1, locked.MineNumber);
            Assert.AreEqual(0, locked.CartLevel);
            Assert.AreEqual(1, locked.Next!.Level);
            Assert.AreEqual(150, locked.Next.Capacity);
            Assert.AreEqual(300, locked.Next.Cost.Single().Quantity);
            Assert.IsFalse(locked.Next.CanAfford);
            Assert.IsFalse(locked.Next.SkillAllows);

            await SetSkillAsync(SkillType.CartCapacity, 1);
            await GiveOresAsync(OreType.Quartz, 300);
            var open = (await Players.GetStateAsync(7)).CartUpgrades.Single();
            Assert.IsTrue(open.Next!.CanAfford);
            Assert.IsTrue(open.Next.SkillAllows);

            await Mines.UpgradeCartAsync(7, 1);
            var after = (await Players.GetStateAsync(7)).CartUpgrades.Single();
            Assert.AreEqual(1, after.CartLevel);
            Assert.AreEqual(2, after.Next!.Level);
            Assert.IsFalse(after.Next.SkillAllows, "a skill está no nível 1");
        }

        [TestMethod]
        public async Task State_NextIsNull_AtTheMaximumLevel()
        {
            await SeedPlayerAsync();
            await using (var ctx = Db.CreateDbContext())
            {
                var mine = await ctx.Mine.SingleAsync();
                mine.CartLevel = 5;
                mine.CartCapacity = 600;
                await ctx.SaveChangesAsync();
            }

            Assert.IsNull((await Players.GetStateAsync(7)).CartUpgrades.Single().Next);
        }

        [TestMethod]
        public async Task SpendingSkillPoints_OnCartCapacity_StopsAtLevelFive()
        {
            await SeedPlayerAsync();
            await SetSkillAsync(SkillType.CartCapacity, 4);
            await using (var ctx = Db.CreateDbContext())
            {
                (await ctx.Player.SingleAsync()).SkillPoints = 2;
                await ctx.SaveChangesAsync();
            }

            var fifth = await Skills.DistributeAsync(7, "CartCapacity");
            Assert.AreEqual(5, fifth.Skills.Single(s => s.Type == SkillType.CartCapacity).Level);

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Skills.DistributeAsync(7, "CartCapacity"));
            Assert.AreEqual(ShardsErrorCode.SkillMaxLevel, ex.Code);
            Assert.AreEqual(1, (await PlayerRow()).SkillPoints);
        }
    }
}
