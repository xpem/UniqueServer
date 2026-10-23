using Microsoft.EntityFrameworkCore;
using Shards.Config;
using Shards.Errors;
using Shards.Model.DTO;

namespace ShardsTests
{
    [TestClass]
    public class MineSlotsRulesTests : GameRulesTestBase
    {
        [TestMethod]
        public void Slots_AreBasePlusSkillLevel_CappedAtFive()
        {
            Assert.AreEqual(1, Rules.MineSlotsFor(0));
            Assert.AreEqual(2, Rules.MineSlotsFor(1));
            Assert.AreEqual(5, Rules.MineSlotsFor(4));
            Assert.AreEqual(5, Rules.MineSlotsFor(99), "acima do nível máximo não passa de 5 minas");
            Assert.AreEqual(1, Rules.MineSlotsFor(-3));
        }

        [TestMethod]
        public void MaxSkillLevel_OfMineSlots_ComesFromTheConfiguration()
        {
            Assert.AreEqual(4, Rules.MaxSkillLevel(SkillType.MineSlots));
            Assert.AreEqual(5, Rules.MaxSkillLevel(SkillType.DoubleDrop));
        }

        [TestMethod]
        public void AppSettings_HasCostsForMinesTwoToFive()
        {
            Assert.AreEqual(1, Balance.MineSlots.BaseSlots);
            Assert.AreEqual(4, Balance.MineSlots.MaxLevel);

            int[] expected = [200, 600, 1800, 5000];
            for (int i = 0; i < expected.Length; i++)
            {
                var cost = Rules.GetMine(i + 2).UnlockCost.Single();
                Assert.AreEqual(OreType.Quartz, cost.Ore);
                Assert.AreEqual(expected[i], cost.Quantity, $"Mina {i + 2}");
            }
        }

        [TestMethod]
        public void Validator_RejectsInvalidSlotSettings()
        {
            var validator = new GameBalanceOptionsValidator();

            Balance.MineSlots.BaseSlots = 0;
            Assert.IsTrue(validator.Validate(null, Balance).Failures!.Any(f => f.Contains("BaseSlots")));

            Balance.MineSlots.BaseSlots = 1;
            Balance.MineSlots.MaxLevel = -1;
            Assert.IsTrue(validator.Validate(null, Balance).Failures!.Any(f => f.Contains("MineSlots.MaxLevel")));
        }
    }

    [TestClass]
    public class MineSlotsServiceTests : ShardsServiceTestBase
    {
        private async Task SetSkillPointsAsync(int points)
        {
            await using var ctx = Db.CreateDbContext();
            (await ctx.Player.SingleAsync()).SkillPoints = points;
            await ctx.SaveChangesAsync();
        }

        [TestMethod]
        public async Task NewPlayer_HasOneMineAndOneSlot()
        {
            var state = await Players.GetStateAsync(7);

            Assert.AreEqual(1, state.MineSlots.Used);
            Assert.AreEqual(1, state.MineSlots.Total);
            Assert.IsFalse(state.NextMine!.HasSlot);
            Assert.AreEqual(0, state.Skills.Single(s => s.Type == SkillType.MineSlots).Level);
        }

        [TestMethod]
        public async Task UnlockWithoutSlot_IsMineSlotsFull_AndDoesNotCharge()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 200);

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.UnlockNextAsync(7));

            Assert.AreEqual(ShardsErrorCode.MineSlotsFull, ex.Code);
            Assert.AreEqual(200, await InventoryOf(OreType.Quartz));
            await using var ctx = Db.CreateDbContext();
            Assert.AreEqual(1, await ctx.Mine.CountAsync());
        }

        [TestMethod]
        public async Task SlotAlone_IsNotEnough_QuartzIsStillCharged()
        {
            await SeedPlayerAsync();
            await GrantMineSlotAsync();

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.UnlockNextAsync(7));

            Assert.AreEqual(ShardsErrorCode.NotEnoughResources, ex.Code);
        }

        [TestMethod]
        public async Task SpendingAPointOnMineSlots_FreesTheSlot()
        {
            await SeedPlayerAsync();
            await SetSkillPointsAsync(1);
            await GiveOresAsync(OreType.Quartz, 200);

            var points = await Skills.DistributeAsync(7, "MineSlots");
            var state = await Players.GetStateAsync(7);

            Assert.AreEqual(1, points.Skills.Single(s => s.Type == SkillType.MineSlots).Level);
            Assert.AreEqual(0, points.SkillPoints);
            Assert.AreEqual(2, state.MineSlots.Total);
            Assert.IsTrue(state.NextMine!.HasSlot);
            Assert.IsTrue(state.NextMine.CanAfford);

            var unlocked = await Mines.UnlockNextAsync(7);

            Assert.AreEqual(2, unlocked.Mine.MineNumber);
            Assert.AreEqual(2, (await Players.GetStateAsync(7)).MineSlots.Used);
        }

        [TestMethod]
        public async Task FirstPointOnMineSlots_CompletesTheSpecializationMission()
        {
            await SeedPlayerAsync();
            await CompleteAndClaimFirstMissionAsync();

            var res = await Skills.DistributeAsync(7, "MineSlots");

            Assert.AreEqual(MissionType.Specialization, res.Missions.Single().Type);
            Assert.AreEqual(MissionStatus.Completed, res.Missions.Single().Status);
        }

        [TestMethod]
        public async Task TheLimitIsFiveMines_WithTheConfiguredCosts()
        {
            await SeedPlayerAsync();
            await GrantMineSlotAsync(levels: 4);
            await GiveOresAsync(OreType.Quartz, 200 + 600 + 1800 + 5000 + 7);

            int[] costs = [200, 600, 1800, 5000];
            for (int i = 0; i < costs.Length; i++)
            {
                int before = await InventoryOf(OreType.Quartz);
                var res = await Mines.UnlockNextAsync(7);

                Assert.AreEqual(i + 2, res.Mine.MineNumber);
                Assert.AreEqual(before - costs[i], await InventoryOf(OreType.Quartz), $"custo da Mina {i + 2}");
            }

            var state = await Players.GetStateAsync(7);
            Assert.AreEqual(5, state.Mines.Count);
            Assert.AreEqual(5, state.MineSlots.Used);
            Assert.AreEqual(5, state.MineSlots.Total);
            Assert.IsNull(state.NextMine);

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.UnlockNextAsync(7));
            Assert.AreEqual(ShardsErrorCode.NoMoreMines, ex.Code);
            Assert.AreEqual(7, await InventoryOf(OreType.Quartz));
        }

        [TestMethod]
        public async Task MineSlots_StopsAtLevelFour()
        {
            await SeedPlayerAsync();
            await GrantMineSlotAsync(levels: 4);
            await SetSkillPointsAsync(1);

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Skills.DistributeAsync(7, "MineSlots"));

            Assert.AreEqual(ShardsErrorCode.SkillMaxLevel, ex.Code);
            Assert.AreEqual(1, (await PlayerRow()).SkillPoints, "o ponto não é gasto");
        }

        [TestMethod]
        public async Task PlayerWithMoreMinesThanSlots_KeepsThem_ButCannotBuyMore()
        {
            await SeedPlayerAsync();
            await GrantMineSlotAsync();
            await GiveOresAsync(OreType.Quartz, 200);
            await Mines.UnlockNextAsync(7);
            await using (var ctx = Db.CreateDbContext()) // simula uma conta antiga: a skill nunca foi gasta
            {
                ctx.PlayerSkill.RemoveRange(ctx.PlayerSkill);
                await ctx.SaveChangesAsync();
            }
            await GiveOresAsync(OreType.Quartz, 600);

            var state = await Players.GetStateAsync(7);

            Assert.AreEqual(2, state.Mines.Count, "as minas existentes são mantidas");
            Assert.AreEqual(2, state.MineSlots.Used);
            Assert.AreEqual(2, state.MineSlots.Total, "o total nunca fica abaixo das minas que o jogador já tem");
            Assert.IsFalse(state.NextMine!.HasSlot);

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.UnlockNextAsync(7));
            Assert.AreEqual(ShardsErrorCode.MineSlotsFull, ex.Code);

            var mined = await Mines.MineAsync(7, 2, 1);
            Assert.AreEqual(1, mined.Drops.Single().Quantity, "a mina antiga continua funcionando");
        }

        [TestMethod]
        public async Task SlotsAreSeparatePerPlayer()
        {
            await SeedPlayerAsync(uid: 7);
            await SeedPlayerAsync(uid: 8);
            await GrantMineSlotAsync(uid: 7);

            var other = await Players.GetStateAsync(8);

            Assert.AreEqual(1, other.MineSlots.Total);
            Assert.AreEqual(2, (await Players.GetStateAsync(7)).MineSlots.Total);
        }
    }
}
