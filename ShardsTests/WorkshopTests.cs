using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shards.Config;
using Shards.Errors;
using Shards.Model.DTO;
using Shards.Service;

namespace ShardsTests
{
    [TestClass]
    public class WorkshopRulesTests : GameRulesTestBase
    {
        [TestMethod]
        public void AppSettings_Recipes_MatchThePlan()
        {
            var polished = Rules.FindWorkshopStep(ItemStage.Polished)!;
            Assert.AreEqual(1, polished.Level);
            Assert.AreEqual(5, polished.InputQuantity);
            Assert.AreEqual(100, polished.Cost.Single().Quantity);

            var faceted = Rules.FindWorkshopStep(ItemStage.Faceted)!;
            Assert.AreEqual(2, faceted.Level);
            Assert.AreEqual(3, faceted.InputQuantity);
            Assert.AreEqual(400, faceted.Cost.Single().Quantity);

            Assert.IsNull(Rules.FindWorkshopStep(ItemStage.Raw));
        }

        [TestMethod]
        public void SkillMaxLevel_IsTheHighestConfiguredRecipe()
        {
            Assert.AreEqual(2, Rules.MaxSkillLevel(SkillType.Workshop));
        }

        [TestMethod]
        public void Validator_AcceptsTheRealAppSettings()
        {
            Assert.IsTrue(new GameBalanceOptionsValidator().Validate(null, Balance).Succeeded);
        }

        [TestMethod]
        public void Validator_RejectsBrokenRecipes()
        {
            Balance.Workshop[1].Stage = ItemStage.Polished; // nível 2 tem de produzir Faceted
            Balance.Workshop[0].InputQuantity = 0;

            var result = new GameBalanceOptionsValidator().Validate(null, Balance);

            Assert.IsTrue(result.Failed);
            Assert.IsTrue(result.Failures.Any(f => f.Contains("Workshop[2]")));
            Assert.IsTrue(result.Failures.Any(f => f.Contains("Workshop[1]")));
        }

        [TestMethod]
        public void ParseGem_RejectsQuartzAndGarbage()
        {
            Assert.AreEqual(OreType.Ruby, ShardsErrors.ParseGem("ruby"));
            foreach (string? bad in new[] { "Quartz", "Nope", "", null, "4" })
                Assert.AreEqual(ShardsErrorCode.InvalidGem, Assert.ThrowsExactly<ShardsException>(() => ShardsErrors.ParseGem(bad)).Code);
        }

        [TestMethod]
        public void ParseCraftStage_RejectsRawAndGarbage()
        {
            Assert.AreEqual(ItemStage.Faceted, ShardsErrors.ParseCraftStage("faceted"));
            foreach (string? bad in new[] { "Raw", "Nope", "", null, "1" })
                Assert.AreEqual(ShardsErrorCode.InvalidStage, Assert.ThrowsExactly<ShardsException>(() => ShardsErrors.ParseCraftStage(bad)).Code);
        }
    }

    [TestClass]
    public class WorkshopServiceTests : ShardsServiceTestBase
    {
        private WorkshopService Workshop => new(Db, Rules, new ShardsMapper(Rules));

        private async Task SetWorkshopSkillAsync(int level, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            ctx.PlayerSkill.Add(new PlayerSkillDTO { PlayerId = player.Id, SkillType = SkillType.Workshop, Level = level });
            await ctx.SaveChangesAsync();
        }

        private async Task GiveStagedAsync(OreType ore, ItemStage stage, int quantity, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            ctx.InventoryItem.Add(new InventoryItemDTO { PlayerId = player.Id, OreType = ore, Stage = stage, Quantity = quantity });
            await ctx.SaveChangesAsync();
        }

        private async Task<int> StageQuantity(OreType ore, ItemStage stage, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            return await ctx.InventoryItem.Where(i => i.PlayerId == player.Id && i.OreType == ore && i.Stage == stage)
                .Select(i => i.Quantity).FirstOrDefaultAsync();
        }

        [TestMethod]
        public async Task Polish_ConsumesRawGemsAndQuartz_AndProducesPolished()
        {
            await SeedPlayerAsync();
            await SetWorkshopSkillAsync(1);
            await GiveOresAsync(OreType.Ruby, 12);
            await GiveOresAsync(OreType.Quartz, 250);

            var res = await Workshop.CraftAsync(7, "Ruby", "Polished", 2);

            Assert.AreEqual(2, res.Crafted.Quantity);
            Assert.AreEqual(ItemStage.Polished, res.Crafted.Stage);
            Assert.AreEqual(2, await StageQuantity(OreType.Ruby, ItemStage.Polished));
            Assert.AreEqual(2, await StageQuantity(OreType.Ruby, ItemStage.Raw), "12 - 2*5");
            Assert.AreEqual(50, await StageQuantity(OreType.Quartz, ItemStage.Raw), "250 - 2*100");
            Assert.AreEqual(2, res.Spent.Count);
            Assert.IsTrue(res.Inventory.Items.Any(i => i.OreType == OreType.Ruby && i.Stage == ItemStage.Polished && i.Quantity == 2));
        }

        [TestMethod]
        public async Task Facet_ConsumesPolishedGems()
        {
            await SeedPlayerAsync();
            await SetWorkshopSkillAsync(2);
            await GiveStagedAsync(OreType.Sapphire, ItemStage.Polished, 3);
            await GiveOresAsync(OreType.Quartz, 400);

            await Workshop.CraftAsync(7, "Sapphire", "Faceted", 1);

            Assert.AreEqual(1, await StageQuantity(OreType.Sapphire, ItemStage.Faceted));
            Assert.AreEqual(0, await StageQuantity(OreType.Sapphire, ItemStage.Polished));
            Assert.AreEqual(0, await StageQuantity(OreType.Quartz, ItemStage.Raw));
        }

        [TestMethod]
        public async Task WithoutTheSkillLevel_IsLocked_AndChargesNothing()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Ruby, 5);
            await GiveOresAsync(OreType.Quartz, 100);

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Workshop.CraftAsync(7, "Ruby", "Polished", 1));
            Assert.AreEqual(ShardsErrorCode.WorkshopLocked, ex.Code);

            await SetWorkshopSkillAsync(1);
            await GiveStagedAsync(OreType.Ruby, ItemStage.Polished, 3);
            var ex2 = await Assert.ThrowsExactlyAsync<ShardsException>(() => Workshop.CraftAsync(7, "Ruby", "Faceted", 1));
            Assert.AreEqual(ShardsErrorCode.WorkshopLocked, ex2.Code, "nível 1 não libera Faceted");

            Assert.AreEqual(5, await StageQuantity(OreType.Ruby, ItemStage.Raw));
            Assert.AreEqual(100, await StageQuantity(OreType.Quartz, ItemStage.Raw));
        }

        [TestMethod]
        public async Task NotEnoughGems_OrNotEnoughQuartz_ChangesNothing()
        {
            await SeedPlayerAsync();
            await SetWorkshopSkillAsync(1);
            await GiveOresAsync(OreType.Ruby, 4); // falta 1 gema
            await GiveOresAsync(OreType.Quartz, 1000);

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Workshop.CraftAsync(7, "Ruby", "Polished", 1));
            Assert.AreEqual(ShardsErrorCode.NotEnoughResources, ex.Code);

            await GiveOresAsync(OreType.Ruby, 1);
            var ex2 = await Assert.ThrowsExactlyAsync<ShardsException>(() => Workshop.CraftAsync(7, "Ruby", "Polished", 11)); // quartzo: 1100 > 1000
            Assert.AreEqual(ShardsErrorCode.NotEnoughResources, ex2.Code);

            Assert.AreEqual(5, await StageQuantity(OreType.Ruby, ItemStage.Raw));
            Assert.AreEqual(1000, await StageQuantity(OreType.Quartz, ItemStage.Raw));
            Assert.AreEqual(0, await StageQuantity(OreType.Ruby, ItemStage.Polished));
        }

        [TestMethod]
        public async Task PolishedQuartz_DoesNotPayTheCost()
        {
            await SeedPlayerAsync();
            await SetWorkshopSkillAsync(1);
            await GiveOresAsync(OreType.Ruby, 5);
            await GiveStagedAsync(OreType.Quartz, ItemStage.Polished, 1000);

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Workshop.CraftAsync(7, "Ruby", "Polished", 1));

            Assert.AreEqual(ShardsErrorCode.NotEnoughResources, ex.Code);
        }

        [TestMethod]
        public async Task InvalidInputs_AreRejectedBeforeAnything()
        {
            Assert.AreEqual(ShardsErrorCode.InvalidGem, (await Assert.ThrowsExactlyAsync<ShardsException>(() => Workshop.CraftAsync(7, "Quartz", "Polished", 1))).Code);
            Assert.AreEqual(ShardsErrorCode.InvalidStage, (await Assert.ThrowsExactlyAsync<ShardsException>(() => Workshop.CraftAsync(7, "Ruby", "Raw", 1))).Code);
            Assert.AreEqual(ShardsErrorCode.InvalidTimes, (await Assert.ThrowsExactlyAsync<ShardsException>(() => Workshop.CraftAsync(7, "Ruby", "Polished", 0))).Code);
            Assert.AreEqual(ShardsErrorCode.InvalidTimes, (await Assert.ThrowsExactlyAsync<ShardsException>(() => Workshop.CraftAsync(7, "Ruby", "Polished", 101))).Code);
        }

        [TestMethod]
        public async Task WithoutPlayer_ReturnsPlayerNotFound()
        {
            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Workshop.CraftAsync(99, "Ruby", "Polished", 1));

            Assert.AreEqual(ShardsErrorCode.PlayerNotFound, ex.Code);
        }

        [TestMethod]
        public async Task State_ListsRecipes_WithSkillAndAffordability()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Ruby, 5);
            await GiveOresAsync(OreType.Quartz, 100);
            await GiveStagedAsync(OreType.Sapphire, ItemStage.Polished, 3);

            var state = await Players.GetStateAsync(7);

            Assert.AreEqual(2, state.Workshop.Count);
            var polished = state.Workshop[0];
            Assert.AreEqual(ItemStage.Polished, polished.Stage);
            Assert.AreEqual(5, polished.InputQuantity);
            Assert.AreEqual(100, polished.Cost.Single().Quantity);
            Assert.IsFalse(polished.SkillAllows, "sem a skill");
            Assert.AreEqual(3, polished.Gems.Count, "Ruby, Sapphire, Emerald (sem Quartz)");
            Assert.IsTrue(polished.Gems.Single(g => g.OreType == OreType.Ruby).CanAfford);
            Assert.AreEqual(5, polished.Gems.Single(g => g.OreType == OreType.Ruby).Available);
            Assert.IsFalse(polished.Gems.Single(g => g.OreType == OreType.Sapphire).CanAfford);

            var faceted = state.Workshop[1];
            Assert.AreEqual(3, faceted.Gems.Single(g => g.OreType == OreType.Sapphire).Available, "conta o estágio Polished");
            Assert.IsFalse(faceted.Gems.Single(g => g.OreType == OreType.Sapphire).CanAfford, "falta quartzo (400)");

            await SetWorkshopSkillAsync(1);
            state = await Players.GetStateAsync(7);
            Assert.IsTrue(state.Workshop[0].SkillAllows);
            Assert.IsFalse(state.Workshop[1].SkillAllows);
        }

        [TestMethod]
        public async Task WorkshopSkill_CanBeBoughtWithPoints_UpToLevelTwo()
        {
            await SeedPlayerAsync();
            await using (var ctx = Db.CreateDbContext())
            {
                var player = await ctx.Player.SingleAsync();
                player.SkillPoints = 3;
                await ctx.SaveChangesAsync();
            }

            await Skills.DistributeAsync(7, "Workshop");
            var second = await Skills.DistributeAsync(7, "Workshop");
            Assert.AreEqual(2, second.Skills.Single(s => s.Type == SkillType.Workshop).Level);

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Skills.DistributeAsync(7, "Workshop"));
            Assert.AreEqual(ShardsErrorCode.SkillMaxLevel, ex.Code);
        }
    }
}
