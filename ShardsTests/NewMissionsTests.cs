using Microsoft.EntityFrameworkCore;
using Shards.Model.DTO;
using Shards.Model.Res;
using Shards.Rules;
using Shards.Service;

namespace ShardsTests
{
    [TestClass]
    public class MissionConditionTests
    {
        private static InventoryItemDTO Row(OreType ore, ItemStage stage, int quantity = 1) =>
            new() { PlayerId = 1, OreType = ore, Stage = stage, Quantity = quantity };

        [TestMethod]
        public void StageMissions_UseRowExistence_NotQuantity()
        {
            // o polido foi todo transformado em facetado: a linha Polished segue lá com 0
            var facts = new MissionFacts(Inventory: [Row(OreType.Ruby, ItemStage.Polished, 0), Row(OreType.Ruby, ItemStage.Faceted, 1)]);

            Assert.IsTrue(MissionTracker.IsSatisfied(MissionType.FirstPolish, facts));
            Assert.IsTrue(MissionTracker.IsSatisfied(MissionType.FirstFacet, facts));
        }

        [TestMethod]
        public void FirstPolish_IsNotSatisfiedByRawGems()
        {
            var facts = new MissionFacts(Inventory: [Row(OreType.Ruby, ItemStage.Raw, 50)]);

            Assert.IsFalse(MissionTracker.IsSatisfied(MissionType.FirstPolish, facts));
            Assert.IsFalse(MissionTracker.IsSatisfied(MissionType.FirstFacet, facts));
        }

        [TestMethod]
        public void Collector_NeedsAllThreeGems_InAnyStage_AndIgnoresQuartz()
        {
            var two = new MissionFacts(Inventory: [Row(OreType.Ruby, ItemStage.Raw), Row(OreType.Sapphire, ItemStage.Polished), Row(OreType.Quartz, ItemStage.Raw)]);
            var three = new MissionFacts(Inventory: [Row(OreType.Ruby, ItemStage.Raw), Row(OreType.Sapphire, ItemStage.Polished), Row(OreType.Emerald, ItemStage.Faceted)]);

            Assert.IsFalse(MissionTracker.IsSatisfied(MissionType.Collector, two));
            Assert.IsTrue(MissionTracker.IsSatisfied(MissionType.Collector, three));
        }

        [TestMethod]
        public void FullSet_NeedsAFacetedGemOfEachType()
        {
            var raw = new MissionFacts(Inventory: [Row(OreType.Ruby, ItemStage.Faceted), Row(OreType.Sapphire, ItemStage.Faceted), Row(OreType.Emerald, ItemStage.Polished)]);
            var full = new MissionFacts(Inventory: [Row(OreType.Ruby, ItemStage.Faceted), Row(OreType.Sapphire, ItemStage.Faceted), Row(OreType.Emerald, ItemStage.Faceted)]);

            Assert.IsFalse(MissionTracker.IsSatisfied(MissionType.FullSet, raw));
            Assert.IsTrue(MissionTracker.IsSatisfied(MissionType.FullSet, full));
        }

        [TestMethod]
        public void Diversification_NeedsTwoDifferentGems_NotTwoMinesOnTheSameGem()
        {
            MineDTO Mine(int n, OreType production) => new() { PlayerId = 1, MineNumber = n, CreatedAt = DateTime.UtcNow, LastCartCollectionUtc = DateTime.UtcNow, ProductionType = production };

            Assert.IsFalse(MissionTracker.IsSatisfied(MissionType.Diversification, new MissionFacts(Mines: [Mine(1, OreType.Ruby), Mine(2, OreType.Ruby)])));
            Assert.IsFalse(MissionTracker.IsSatisfied(MissionType.Diversification, new MissionFacts(Mines: [Mine(1, OreType.Ruby), Mine(2, OreType.Quartz)])));
            Assert.IsTrue(MissionTracker.IsSatisfied(MissionType.Diversification, new MissionFacts(Mines: [Mine(1, OreType.Ruby), Mine(2, OreType.Emerald)])));
        }

        [TestMethod]
        public void MissingFacts_NeverSatisfy()
        {
            foreach (MissionType type in Enum.GetValues<MissionType>())
                Assert.IsFalse(MissionTracker.IsSatisfied(type, new MissionFacts()), type.ToString());
        }

        [TestMethod]
        public void CountMissions_AreNeverSatisfiedByState()
        {
            var facts = new MissionFacts(
                Skills: [new PlayerSkillDTO { PlayerId = 1, SkillType = SkillType.Workshop, Level = 2 }],
                Inventory: [Row(OreType.Ruby, ItemStage.Faceted, 99)]);

            Assert.IsFalse(MissionTracker.IsSatisfied(MissionType.PolishBatch, facts));
            Assert.IsFalse(MissionTracker.IsSatisfied(MissionType.FacetBatch, facts));
            Assert.IsFalse(MissionTracker.IsSatisfied(MissionType.FullCart, facts));
        }
    }

    [TestClass]
    public class NewMissionsFlowTests : ShardsServiceTestBase
    {
        private WorkshopService Workshop => new(Db, Rules, new ShardsMapper(Rules));

        /// <summary>Deixa as missões anteriores à informada como resgatadas e a informada ativa (atalho para testar uma missão isolada).</summary>
        private async Task ActivateAsync(MissionType type, int progress = 0, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            ctx.PlayerMission.RemoveRange(await ctx.PlayerMission.Where(m => m.PlayerId == player.Id).ToListAsync());

            foreach (var def in MissionCatalog.All)
            {
                if (def.Type == type)
                {
                    ctx.PlayerMission.Add(new PlayerMissionDTO { PlayerId = player.Id, MissionType = type, Status = MissionStatus.Active, Progress = progress });
                    break;
                }

                ctx.PlayerMission.Add(new PlayerMissionDTO { PlayerId = player.Id, MissionType = def.Type, Status = MissionStatus.Claimed });
            }

            await ctx.SaveChangesAsync();
        }

        private async Task<PlayerMissionDTO> MissionRow(MissionType type)
        {
            await using var ctx = Db.CreateDbContext();
            return await ctx.PlayerMission.AsNoTracking().SingleAsync(m => m.MissionType == type);
        }

        private async Task SetSkillAsync(SkillType type, int level, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            var skill = await ctx.PlayerSkill.FirstOrDefaultAsync(s => s.PlayerId == player.Id && s.SkillType == type);
            if (skill is null) ctx.PlayerSkill.Add(new PlayerSkillDTO { PlayerId = player.Id, SkillType = type, Level = level });
            else skill.Level = level;
            await ctx.SaveChangesAsync();
        }

        private async Task GivePointsAsync(int points, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            (await ctx.Player.SingleAsync(p => p.UserId == uid)).SkillPoints = points;
            await ctx.SaveChangesAsync();
        }

        private async Task GiveStagedAsync(OreType ore, ItemStage stage, int quantity, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            ctx.InventoryItem.Add(new InventoryItemDTO { PlayerId = player.Id, OreType = ore, Stage = stage, Quantity = quantity });
            await ctx.SaveChangesAsync();
        }

        private async Task AddMineAsync(int number, OreType production = OreType.Quartz, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            ctx.Mine.Add(new MineDTO { PlayerId = player.Id, MineNumber = number, CreatedAt = Start, LastCartCollectionUtc = Start, ProductionType = production });
            await ctx.SaveChangesAsync();
        }

        private async Task SetProductionDirectAsync(int mineNumber, OreType production, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            (await ctx.Mine.SingleAsync(m => m.PlayerId == player.Id && m.MineNumber == mineNumber)).ProductionType = production;
            await ctx.SaveChangesAsync();
        }

        private static void AssertCompleted(IEnumerable<MissionRes> missions, MissionType type) =>
            Assert.AreEqual(MissionStatus.Completed, missions.Single(m => m.Type == type).Status, type.ToString());

        [TestMethod]
        public async Task CartTraining_CompletesWhenSpendingAPointOnCartCapacity()
        {
            await SeedPlayerAsync();
            await ActivateAsync(MissionType.CartTraining);
            await GivePointsAsync(2);

            var other = await Skills.DistributeAsync(7, "DoubleDrop");
            Assert.AreEqual(0, other.Missions.Count, "outra skill não conta");

            var res = await Skills.DistributeAsync(7, "CartCapacity");

            AssertCompleted(res.Missions, MissionType.CartTraining);
        }

        [TestMethod]
        public async Task OpenWorkshop_CompletesWhenSpendingAPointOnWorkshop()
        {
            await SeedPlayerAsync();
            await ActivateAsync(MissionType.OpenWorkshop);
            await GivePointsAsync(1);

            var res = await Skills.DistributeAsync(7, "Workshop");

            AssertCompleted(res.Missions, MissionType.OpenWorkshop);
        }

        [TestMethod]
        public async Task DoubleLuck_CompletesOnlyAtLevelThree()
        {
            await SeedPlayerAsync();
            await ActivateAsync(MissionType.DoubleLuck);
            await SetSkillAsync(SkillType.DoubleDrop, 1);
            await GivePointsAsync(2);

            var second = await Skills.DistributeAsync(7, "DoubleDrop");
            Assert.AreEqual(0, second.Missions.Count, "nível 2 ainda não");

            var third = await Skills.DistributeAsync(7, "DoubleDrop");
            AssertCompleted(third.Missions, MissionType.DoubleLuck);
        }

        [TestMethod]
        public async Task CartUpgrade_CompletesOnUpgrade_AndTheResponseCarriesTheMission()
        {
            await SeedPlayerAsync();
            await ActivateAsync(MissionType.CartUpgrade);
            await SetSkillAsync(SkillType.CartCapacity, 1);
            await GiveOresAsync(OreType.Quartz, 300);

            var res = await Mines.UpgradeCartAsync(7, 1);

            AssertCompleted(res.Missions, MissionType.CartUpgrade);
        }

        [TestMethod]
        public async Task FirstPolish_AndFirstFacet_CompleteOnTheirCrafts()
        {
            await SeedPlayerAsync();
            await SetSkillAsync(SkillType.Workshop, 2);
            await GiveOresAsync(OreType.Ruby, 20);
            await GiveOresAsync(OreType.Quartz, 2000);

            await ActivateAsync(MissionType.FirstPolish);
            var polish = await Workshop.CraftAsync(7, "Ruby", "Polished", 3);
            AssertCompleted(polish.Missions, MissionType.FirstPolish);

            await ActivateAsync(MissionType.FirstFacet);
            var facet = await Workshop.CraftAsync(7, "Ruby", "Faceted", 1);
            AssertCompleted(facet.Missions, MissionType.FirstFacet);
        }

        [TestMethod]
        public async Task FirstFacet_DoesNotCompleteWithAPolishCraft()
        {
            await SeedPlayerAsync();
            await ActivateAsync(MissionType.FirstFacet);
            await SetSkillAsync(SkillType.Workshop, 1);
            await GiveOresAsync(OreType.Ruby, 5);
            await GiveOresAsync(OreType.Quartz, 100);

            var res = await Workshop.CraftAsync(7, "Ruby", "Polished", 1);

            Assert.AreEqual(0, res.Missions.Count);
        }

        [TestMethod]
        public async Task PolishBatch_CountsOnlyPolishCrafts_AndCompletesAtTen()
        {
            await SeedPlayerAsync();
            await ActivateAsync(MissionType.PolishBatch);
            await SetSkillAsync(SkillType.Workshop, 2);
            await GiveOresAsync(OreType.Ruby, 50);
            await GiveOresAsync(OreType.Quartz, 2000);

            var first = await Workshop.CraftAsync(7, "Ruby", "Polished", 4);
            var progress = first.Missions.Single();
            Assert.AreEqual(4, progress.Progress);
            Assert.AreEqual(MissionStatus.Active, progress.Status);

            var facet = await Workshop.CraftAsync(7, "Ruby", "Faceted", 1);
            Assert.AreEqual(0, facet.Missions.Count, "facetar não conta como polimento");

            var rest = await Workshop.CraftAsync(7, "Ruby", "Polished", 6);
            Assert.AreEqual(10, rest.Missions.Single().Progress);
            AssertCompleted(rest.Missions, MissionType.PolishBatch);
        }

        [TestMethod]
        public async Task FacetBatch_CountsFacetCrafts_AndCompletesAtFive()
        {
            await SeedPlayerAsync();
            await ActivateAsync(MissionType.FacetBatch);
            await SetSkillAsync(SkillType.Workshop, 2);
            await GiveStagedAsync(OreType.Sapphire, ItemStage.Polished, 15);
            await GiveOresAsync(OreType.Quartz, 2000);

            var res = await Workshop.CraftAsync(7, "Sapphire", "Faceted", 5);

            Assert.AreEqual(5, res.Missions.Single().Progress);
            AssertCompleted(res.Missions, MissionType.FacetBatch);
        }

        [TestMethod]
        public async Task FullSet_CompletesWhenTheThirdFacetedTypeIsCrafted()
        {
            await SeedPlayerAsync();
            await ActivateAsync(MissionType.FullSet);
            await SetSkillAsync(SkillType.Workshop, 2);
            await GiveStagedAsync(OreType.Ruby, ItemStage.Faceted, 1);
            await GiveStagedAsync(OreType.Sapphire, ItemStage.Faceted, 1);
            await GiveStagedAsync(OreType.Emerald, ItemStage.Polished, 3);
            await GiveOresAsync(OreType.Quartz, 400);

            var res = await Workshop.CraftAsync(7, "Emerald", "Faceted", 1);

            AssertCompleted(res.Missions, MissionType.FullSet);
        }

        [TestMethod]
        public async Task Collector_CompletesWhenMiningBringsTheLastGem()
        {
            await SeedPlayerAsync();
            await ActivateAsync(MissionType.Collector);
            await GiveOresAsync(OreType.Ruby, 1);
            await GiveOresAsync(OreType.Sapphire, 1);
            await SetProductionDirectAsync(1, OreType.Emerald);
            Random.Value = 0.0; // primeiro item da tabela: a Esmeralda

            var res = await Mines.MineAsync(7, 1, 1);

            AssertCompleted(res.Missions, MissionType.Collector);
        }

        [TestMethod]
        public async Task Collector_CompletesOnCollectCart_AndCarriesTheMissionInTheResponse()
        {
            await SeedPlayerAsync();
            await ActivateAsync(MissionType.Collector);
            await GiveOresAsync(OreType.Ruby, 1);
            await GiveOresAsync(OreType.Sapphire, 1);
            await SetProductionDirectAsync(1, OreType.Emerald);
            Random.Value = 0.0;
            Time.Advance(TimeSpan.FromHours(1));

            var res = await Mines.CollectCartAsync(7, 1);

            AssertCompleted(res.Missions, MissionType.Collector);
        }

        [TestMethod]
        public async Task Diversification_CompletesWhenTheSecondMineSwitchesToAnotherGem()
        {
            await SeedPlayerAsync();
            await ActivateAsync(MissionType.Diversification);
            await AddMineAsync(2);
            await SetProductionDirectAsync(1, OreType.Sapphire);
            await GiveOresAsync(OreType.Quartz, 1200);

            var res = await Mines.SetProductionAsync(7, 2, "Ruby");

            AssertCompleted(res.Missions, MissionType.Diversification);
        }

        [TestMethod]
        public async Task Diversification_DoesNotCompleteWithTheSameGemTwice()
        {
            await SeedPlayerAsync();
            await ActivateAsync(MissionType.Diversification);
            await AddMineAsync(2);
            await SetProductionDirectAsync(1, OreType.Sapphire);
            await GiveOresAsync(OreType.Quartz, 500);

            var res = await Mines.SetProductionAsync(7, 2, "Sapphire");

            Assert.AreEqual(0, res.Missions.Count);
        }

        [TestMethod]
        public async Task ThirdMine_CompletesWhenBuyingTheThirdMine()
        {
            await SeedPlayerAsync();
            await ActivateAsync(MissionType.ThirdMine);
            await AddMineAsync(2);
            await GrantMineSlotAsync(2);
            await GiveOresAsync(OreType.Quartz, 600);

            var res = await Mines.UnlockNextAsync(7);

            AssertCompleted(res.Missions, MissionType.ThirdMine);
        }

        [TestMethod]
        public async Task FullCart_CompletesOnlyWhenTheCartWasFull()
        {
            await SeedPlayerAsync();
            await ActivateAsync(MissionType.FullCart);
            Time.Advance(TimeSpan.FromHours(1));

            var partial = await Mines.CollectCartAsync(7, 1);
            Assert.AreEqual(0, partial.Missions.Count, "vagonete só parcialmente cheio");

            Time.Advance(TimeSpan.FromHours(6)); // > 5 h: 100 de capacidade a 0,33/min
            var full = await Mines.CollectCartAsync(7, 1);
            AssertCompleted(full.Missions, MissionType.FullCart);
        }

        [TestMethod]
        public async Task ClaimingFirstGem_UnlocksCartTraining_AlreadyCompletedWhenTheSkillIsThere()
        {
            await SeedPlayerAsync();
            await ActivateAsync(MissionType.FirstGem);
            await using (var ctx = Db.CreateDbContext())
            {
                (await ctx.PlayerMission.SingleAsync(m => m.MissionType == MissionType.FirstGem)).Status = MissionStatus.Completed;
                await ctx.SaveChangesAsync();
            }
            await SetSkillAsync(SkillType.CartCapacity, 1);

            var res = await Missions.ClaimAsync(7, "FirstGem");

            AssertCompleted(res.Missions, MissionType.CartTraining);
        }

        [TestMethod]
        public async Task ClaimingFirstPolish_UnlocksFirstFacet_AlreadyCompletedWhenAFacetedGemExists()
        {
            await SeedPlayerAsync();
            await ActivateAsync(MissionType.FirstPolish);
            await using (var ctx = Db.CreateDbContext())
            {
                (await ctx.PlayerMission.SingleAsync(m => m.MissionType == MissionType.FirstPolish)).Status = MissionStatus.Completed;
                await ctx.SaveChangesAsync();
            }
            await GiveStagedAsync(OreType.Ruby, ItemStage.Faceted, 1);

            var res = await Missions.ClaimAsync(7, "FirstPolish");

            AssertCompleted(res.Missions, MissionType.FirstFacet);
        }

        [TestMethod]
        public async Task State_CreatesTheNewMissionsForPlayersWhoAlreadyClaimedFirstGem()
        {
            await SeedPlayerAsync();
            await using (var ctx = Db.CreateDbContext())
            {
                var player = await ctx.Player.SingleAsync();
                ctx.PlayerMission.RemoveRange(await ctx.PlayerMission.ToListAsync());
                foreach (var type in new[] { MissionType.FirstPickaxe, MissionType.Specialization, MissionType.OperationalExpansion, MissionType.FirstGem })
                    ctx.PlayerMission.Add(new PlayerMissionDTO { PlayerId = player.Id, MissionType = type, Status = MissionStatus.Claimed });
                await ctx.SaveChangesAsync();
            }

            var state = await Players.GetStateAsync(7);

            Assert.AreEqual(MissionType.CartTraining, state.Missions.Single().Type);
        }

        [TestMethod]
        public async Task Rewards_AreXpAndRawQuartzOnly()
        {
            foreach (var def in MissionCatalog.All.Where(m => m.Type >= MissionType.CartTraining))
                Assert.IsTrue(def.Reward.Ores.All(o => o.Ore == OreType.Quartz), $"{def.Type} não deve pagar gemas");

            Assert.AreEqual(600, MissionCatalog.Get(MissionType.FirstPolish).Reward.Ores.Single().Quantity);
        }
    }
}
