using Microsoft.EntityFrameworkCore;
using Shards.Errors;
using Shards.Model.DTO;
using Shards.Service;

namespace ShardsTests
{
    [TestClass]
    public class InventoryStageTests : ShardsServiceTestBase
    {
        private InventoryService Inventory => new(Db, new ShardsMapper(Rules));

        private async Task AddStagedAsync(OreType ore, ItemStage stage, int quantity, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            ctx.InventoryItem.Add(new InventoryItemDTO { PlayerId = player.Id, OreType = ore, Stage = stage, Quantity = quantity });
            await ctx.SaveChangesAsync();
        }

        [TestMethod]
        public async Task GetInventory_WithoutPlayer_ReturnsEmpty()
        {
            var res = await Inventory.GetAsync(7);

            Assert.AreEqual(0, res.Items.Count);
        }

        [TestMethod]
        public async Task GetInventory_NewPlayer_ReturnsEmpty()
        {
            await SeedPlayerAsync();

            var res = await Inventory.GetAsync(7);

            Assert.AreEqual(0, res.Items.Count);
        }

        [TestMethod]
        public async Task GetInventory_OmitsZeroQuantity_AndOrdersByOreThenStage()
        {
            await SeedPlayerAsync();
            await AddStagedAsync(OreType.Ruby, ItemStage.Faceted, 1);
            await AddStagedAsync(OreType.Ruby, ItemStage.Raw, 12);
            await AddStagedAsync(OreType.Ruby, ItemStage.Polished, 0);
            await AddStagedAsync(OreType.Quartz, ItemStage.Raw, 5);
            await AddStagedAsync(OreType.Sapphire, ItemStage.Polished, 3);

            var items = (await Inventory.GetAsync(7)).Items;

            CollectionAssert.AreEqual(
                new[] { (OreType.Quartz, ItemStage.Raw, 5), (OreType.Ruby, ItemStage.Raw, 12), (OreType.Ruby, ItemStage.Faceted, 1), (OreType.Sapphire, ItemStage.Polished, 3) },
                items.Select(i => (i.OreType, i.Stage, i.Quantity)).ToArray());
        }

        [TestMethod]
        public async Task GetInventory_OtherPlayersItems_AreNotReturned()
        {
            await SeedPlayerAsync(uid: 7);
            await SeedPlayerAsync(uid: 8);
            await AddStagedAsync(OreType.Ruby, ItemStage.Raw, 4, uid: 8);

            Assert.AreEqual(0, (await Inventory.GetAsync(7)).Items.Count);
            Assert.AreEqual(1, (await Inventory.GetAsync(8)).Items.Count);
        }

        [TestMethod]
        public async Task Mine_CreditsRawStage()
        {
            await SeedPlayerAsync();
            Random.Value = 0.0;

            await Mines.MineAsync(7, 1, 3);

            await using var ctx = Db.CreateDbContext();
            var rows = await ctx.InventoryItem.ToListAsync();
            Assert.IsTrue(rows.Count > 0);
            Assert.IsTrue(rows.All(r => r.Stage == ItemStage.Raw));
        }

        [TestMethod]
        public async Task Add_SameOreDifferentStage_KeepsSeparateRows()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Ruby, 2);
            await AddStagedAsync(OreType.Ruby, ItemStage.Polished, 1);

            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync();
            var inventory = await ShardsQueries.GetInventoryAsync(ctx, player.Id);
            InventoryItemService.Add(ctx, inventory, player.Id, OreType.Ruby, 3);
            InventoryItemService.Add(ctx, inventory, player.Id, OreType.Ruby, 4, ItemStage.Polished);
            await ctx.SaveChangesAsync();

            Assert.AreEqual(5, InventoryItemService.QuantityOf(inventory, OreType.Ruby));
            Assert.AreEqual(5, InventoryItemService.QuantityOf(inventory, OreType.Ruby, ItemStage.Polished));
            Assert.AreEqual(2, await ctx.InventoryItem.CountAsync());
        }

        [TestMethod]
        public async Task Costs_IgnoreNonRawStages()
        {
            await SeedPlayerAsync();
            // 1000 de quartzo polido não paga nada: custos do jogo são em minério bruto
            await AddStagedAsync(OreType.Quartz, ItemStage.Polished, 1000);
            await CompleteAndClaimFirstMissionAsync();
            await Skills.DistributeAsync(7, "MineSlots");

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.UnlockNextAsync(7));

            Assert.AreEqual(ShardsErrorCode.NotEnoughResources, ex.Code);
        }

        [TestMethod]
        public async Task State_WithStagedRows_DoesNotBreak_AndShowsOnlyRawInInventory()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 10);
            await AddStagedAsync(OreType.Quartz, ItemStage.Polished, 3);

            var state = await Players.GetStateAsync(7);

            var quartz = state.Inventory.Single(i => i.OreType == OreType.Quartz);
            Assert.AreEqual(10, quartz.Quantity);
        }
    }
}
