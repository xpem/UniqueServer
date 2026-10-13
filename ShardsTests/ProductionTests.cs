using Microsoft.EntityFrameworkCore;
using Shards.Errors;
using Shards.Model.DTO;

namespace ShardsTests
{
    [TestClass]
    public class SetProductionTests : ShardsServiceTestBase
    {
        private async Task<MineDTO> MineRow(int mineNumber = 1)
        {
            await using var ctx = Db.CreateDbContext();
            return await ctx.Mine.AsNoTracking().SingleAsync(m => m.MineNumber == mineNumber);
        }

        [TestMethod]
        public async Task SwitchToSapphire_PaysFiveHundredQuartz_AndChangesTheProduction()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 500);

            var res = await Mines.SetProductionAsync(7, 1, "Sapphire");

            Assert.AreEqual(OreType.Sapphire, res.Mine.Production);
            Assert.AreEqual(500, res.Spent.Single().Quantity);
            Assert.AreEqual(OreType.Quartz, res.Spent.Single().OreType);
            Assert.AreEqual(0, res.Inventory.Count, "o quartzo zerado some da lista");
            Assert.AreEqual(OreType.Sapphire, (await MineRow()).ProductionType);
        }

        [TestMethod]
        public async Task NewMines_StartProducingQuartz()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 200);

            var res = await Mines.UnlockNextAsync(7);

            Assert.AreEqual(OreType.Quartz, res.Mine.Production);
            Assert.AreEqual(OreType.Quartz, (await Players.GetStateAsync(7)).Mines.Single(m => m.MineNumber == 1).Production);
        }

        [TestMethod]
        public async Task ActiveMining_AfterTheSwitch_DropsMostlyTheGem()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 500);
            await Mines.SetProductionAsync(7, 1, "Sapphire");

            Random.Value = 0.0;
            var gem = await Mines.MineAsync(7, 1, 1);
            Assert.AreEqual(OreType.Sapphire, gem.Drops.Single().OreType);
            Assert.AreEqual(3, gem.XpGained, "a gema dá mais XP que o quartzo");

            Random.Value = 0.95;
            var quartz = await Mines.MineAsync(7, 1, 1);
            Assert.AreEqual(OreType.Quartz, quartz.Drops.Single().OreType);
            Assert.AreEqual(1, quartz.XpGained);
        }

        [TestMethod]
        public async Task Cart_AfterTheSwitch_UsesTheNewProduction()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 500);
            await Mines.SetProductionAsync(7, 1, "Sapphire");
            Time.Advance(TimeSpan.FromMinutes(10)); // 3 minérios
            Random.Value = 0.0;

            var res = await Mines.CollectCartAsync(7, 1);

            Assert.AreEqual(OreType.Sapphire, res.Collected.Single().OreType);
            Assert.AreEqual(3, res.Total);
        }

        [TestMethod]
        public async Task Switch_EmptiesTheCartWithTheOldProduction_First()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 500);
            Time.Advance(TimeSpan.FromMinutes(10)); // 3 minérios acumulados em Quartzo
            Random.Value = 0.0;

            var res = await Mines.SetProductionAsync(7, 1, "Sapphire");

            Assert.AreEqual(OreType.Quartz, res.Collected.Single().OreType);
            Assert.AreEqual(3, res.Collected.Single().Quantity);
            Assert.AreEqual(3, res.Inventory.Single().Quantity, "503 de quartzo menos 500 do custo");
            Assert.AreEqual(Time.Now.UtcDateTime, (await MineRow()).LastCartCollectionUtc);
            Assert.AreEqual(0, res.Mine.CartAmount);
        }

        [TestMethod]
        public async Task WhatTheCartDelivers_CanCompleteThePayment()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 498);
            Time.Advance(TimeSpan.FromMinutes(10)); // +3 = 501
            Random.Value = 0.0;

            var res = await Mines.SetProductionAsync(7, 1, "Sapphire");

            Assert.AreEqual(OreType.Sapphire, res.Mine.Production);
            Assert.AreEqual(1, res.Inventory.Single().Quantity);
        }

        [TestMethod]
        public async Task NotEnoughQuartz_ChangesNothing_NotEvenTheCart()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 100);
            Time.Advance(TimeSpan.FromMinutes(10)); // 103 < 500

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.SetProductionAsync(7, 1, "Sapphire"));

            Assert.AreEqual(ShardsErrorCode.NotEnoughResources, ex.Code);
            var mine = await MineRow();
            Assert.AreEqual(OreType.Quartz, mine.ProductionType);
            Assert.AreEqual(Start, mine.LastCartCollectionUtc, "a coleta não é gravada se a troca falha");
            Assert.AreEqual(100, await InventoryOf(OreType.Quartz));
        }

        [TestMethod]
        public async Task BackToQuartz_IsFree()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 500);
            await Mines.SetProductionAsync(7, 1, "Sapphire");

            var res = await Mines.SetProductionAsync(7, 1, "quartz");

            Assert.AreEqual(OreType.Quartz, res.Mine.Production);
            Assert.AreEqual(0, res.Spent.Count);
            Assert.AreEqual(0, await InventoryOf(OreType.Quartz), "nada foi cobrado na volta");
        }

        [TestMethod]
        public async Task SwitchingBetweenGems_PaysTheNewOne()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 500 + 1200);
            await Mines.SetProductionAsync(7, 1, "Sapphire");

            var res = await Mines.SetProductionAsync(7, 1, "Ruby");

            Assert.AreEqual(OreType.Ruby, res.Mine.Production);
            Assert.AreEqual(1200, res.Spent.Single().Quantity);
            Assert.AreEqual(0, await InventoryOf(OreType.Quartz));
        }

        [TestMethod]
        [DataRow("Quartz")]
        [DataRow("Sapphire")]
        public async Task SameProduction_IsRejected(string focus)
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 500);
            if (focus != "Quartz") await Mines.SetProductionAsync(7, 1, focus);
            await GiveOresAsync(OreType.Quartz, 500);
            int before = await InventoryOf(OreType.Quartz);

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.SetProductionAsync(7, 1, focus));

            Assert.AreEqual(ShardsErrorCode.SameProduction, ex.Code);
            Assert.AreEqual(before, await InventoryOf(OreType.Quartz), "não cobra por repetir a mesma produção");
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("Nope")]
        [DataRow("1")]
        [DataRow("Sapphire,Ruby")]
        [DataRow("Silver")]
        public async Task InvalidFocus_IsRejectedBeforeAnything(string? focus)
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 500);

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.SetProductionAsync(7, 1, focus));

            Assert.AreEqual(ShardsErrorCode.InvalidProduction, ex.Code);
            Assert.AreEqual(500, await InventoryOf(OreType.Quartz));
        }

        [TestMethod]
        public async Task UnknownPlayer_AndMineNotOwned_AreRejected()
        {
            var noPlayer = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.SetProductionAsync(99, 1, "Sapphire"));
            Assert.AreEqual(ShardsErrorCode.PlayerNotFound, noPlayer.Code);

            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 500);
            var noMine = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.SetProductionAsync(7, 2, "Sapphire"));
            Assert.AreEqual(ShardsErrorCode.MineNotFound, noMine.Code);
            Assert.AreEqual(500, await InventoryOf(OreType.Quartz));
        }

        [TestMethod]
        public async Task EachMine_HasItsOwnProduction()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 200);
            await Mines.UnlockNextAsync(7);
            await GiveOresAsync(OreType.Quartz, 1200);

            await Mines.SetProductionAsync(7, 2, "Ruby");

            var state = await Players.GetStateAsync(7);
            Assert.AreEqual(OreType.Quartz, state.Mines.Single(m => m.MineNumber == 1).Production);
            Assert.AreEqual(OreType.Ruby, state.Mines.Single(m => m.MineNumber == 2).Production);
        }

        [TestMethod]
        public async Task PayingForOneMine_DoesNotUnlockTheGemForTheOthers()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 200 + 500);
            await Mines.UnlockNextAsync(7);
            await Mines.SetProductionAsync(7, 1, "Sapphire");

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.SetProductionAsync(7, 2, "Sapphire"));

            Assert.AreEqual(ShardsErrorCode.NotEnoughResources, ex.Code, "cada mina paga a sua mudança");
        }
    }

    [TestClass]
    public class ProductionStateTests : ShardsServiceTestBase
    {
        [TestMethod]
        public async Task State_ListsProductionsWithCostAndNormalizedChances()
        {
            var state = await Players.GetStateAsync(7);

            Assert.AreEqual(4, state.Productions.Count);

            var quartz = state.Productions.Single(p => p.Focus == OreType.Quartz);
            Assert.AreEqual(0, quartz.SwitchCost.Count);
            Assert.IsTrue(quartz.CanAfford);

            var sapphire = state.Productions.Single(p => p.Focus == OreType.Sapphire);
            Assert.AreEqual(500, sapphire.SwitchCost.Single().Quantity);
            Assert.AreEqual(0.9, sapphire.Drops.Single(d => d.OreType == OreType.Sapphire).Chance, 1e-9);
            Assert.AreEqual(0.1, sapphire.Drops.Single(d => d.OreType == OreType.Quartz).Chance, 1e-9);
            Assert.IsFalse(sapphire.CanAfford);
            Assert.AreEqual(OreType.Quartz, state.Mines.Single().Production);
        }

        [TestMethod]
        public async Task State_CanAfford_FollowsTheInventory()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 1200);

            var state = await Players.GetStateAsync(7);

            Assert.IsTrue(state.Productions.Single(p => p.Focus == OreType.Sapphire).CanAfford);
            Assert.IsTrue(state.Productions.Single(p => p.Focus == OreType.Ruby).CanAfford);
            Assert.IsFalse(state.Productions.Single(p => p.Focus == OreType.Emerald).CanAfford);
        }
    }

    [TestClass]
    public class FirstGemMissionTests : ShardsServiceTestBase
    {
        /// <summary>Percorre as Missões 1 a 3 e deixa a Missão 4 (Primeira Gema) ativa.</summary>
        private async Task ReachFourthMissionAsync()
        {
            await SeedPlayerAsync();
            await CompleteAndClaimFirstMissionAsync();
            await Skills.DistributeAsync(7, "DoubleDrop");
            await Missions.ClaimAsync(7, "Specialization");
            await GiveOresAsync(OreType.Quartz, 200);
            await Mines.UnlockNextAsync(7);
            await Missions.ClaimAsync(7, "OperationalExpansion");
        }

        [TestMethod]
        public async Task ClaimingTheThird_ReleasesTheFourth()
        {
            await ReachFourthMissionAsync();

            var mission = (await Players.GetStateAsync(7)).Missions.Single();

            Assert.AreEqual(MissionType.FirstGem, mission.Type);
            Assert.AreEqual(MissionStatus.Active, mission.Status);
            Assert.AreEqual(50, mission.Reward.Xp);
        }

        [TestMethod]
        public async Task PuttingAMineOnAGem_CompletesIt()
        {
            await ReachFourthMissionAsync();
            await GiveOresAsync(OreType.Quartz, 500);

            var res = await Mines.SetProductionAsync(7, 1, "Sapphire");

            var mission = res.Missions.Single();
            Assert.AreEqual(MissionType.FirstGem, mission.Type);
            Assert.AreEqual(MissionStatus.Completed, mission.Status);
        }

        [TestMethod]
        public async Task GoingBackToQuartz_DoesNotCount()
        {
            await ReachFourthMissionAsync();
            await GiveOresAsync(OreType.Quartz, 500);
            await Mines.SetProductionAsync(7, 1, "Sapphire");
            // refaz o estado: a missão volta a ativa só para provar que o retorno ao quartzo não a avança
            await using (var ctx = Db.CreateDbContext())
            {
                var mission = await ctx.PlayerMission.SingleAsync(m => m.MissionType == MissionType.FirstGem);
                mission.Status = MissionStatus.Active;
                mission.Progress = 0;
                await ctx.SaveChangesAsync();
            }

            var res = await Mines.SetProductionAsync(7, 1, "Quartz");

            Assert.AreEqual(0, res.Missions.Count);
        }

        [TestMethod]
        public async Task Claiming_GivesFiftyXp_AndEndsTheChain()
        {
            await ReachFourthMissionAsync();
            await GiveOresAsync(OreType.Quartz, 500);
            await Mines.SetProductionAsync(7, 1, "Sapphire");
            int xpBefore = (await PlayerRow()).Experience;

            var res = await Missions.ClaimAsync(7, "FirstGem");

            Assert.AreEqual(50, res.Rewards.Xp);
            Assert.AreEqual(xpBefore + 50, res.Player.Experience);
            Assert.AreEqual(1, res.Missions.Count, "não há missão depois da Primeira Gema");
            Assert.AreEqual(0, (await Players.GetStateAsync(7)).Missions.Count);
        }

        [TestMethod]
        public async Task FourthMission_StartsCompleted_WhenAMineAlreadyProducesAGem()
        {
            await SeedPlayerAsync();
            await CompleteAndClaimFirstMissionAsync();
            await Skills.DistributeAsync(7, "DoubleDrop");
            await Missions.ClaimAsync(7, "Specialization");
            await GiveOresAsync(OreType.Quartz, 200 + 500);
            await Mines.UnlockNextAsync(7);
            await Mines.SetProductionAsync(7, 1, "Sapphire"); // antes de a Missão 4 existir

            var res = await Missions.ClaimAsync(7, "OperationalExpansion");

            Assert.AreEqual(MissionStatus.Completed, res.Missions.Single(m => m.Type == MissionType.FirstGem).Status);
        }

        [TestMethod]
        public async Task State_CreatesTheFourthMission_ForPlayersWhoClaimedTheThirdBeforeItExisted()
        {
            await ReachFourthMissionAsync();
            await using (var ctx = Db.CreateDbContext())
            {
                ctx.PlayerMission.Remove(await ctx.PlayerMission.SingleAsync(m => m.MissionType == MissionType.FirstGem));
                await ctx.SaveChangesAsync();
            }

            var state = await Players.GetStateAsync(7);

            Assert.AreEqual(MissionType.FirstGem, state.Missions.Single().Type);
            await using var after = Db.CreateDbContext();
            Assert.AreEqual(1, await after.PlayerMission.CountAsync(m => m.MissionType == MissionType.FirstGem));
        }

        [TestMethod]
        public async Task State_DoesNotDuplicateMissions_OnRepeatedCalls()
        {
            await ReachFourthMissionAsync();

            await Players.GetStateAsync(7);
            await Players.GetStateAsync(7);

            await using var ctx = Db.CreateDbContext();
            Assert.AreEqual(4, await ctx.PlayerMission.CountAsync());
        }
    }
}
