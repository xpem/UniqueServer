using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shards.Config;
using Shards.Errors;
using Shards.Model.DTO;
using Shards.Model.Req;
using Shards.Service;

namespace ShardsTests
{
    [TestClass]
    public class DepositExtractionTests : ShardsServiceTestBase
    {
        private DepositService Deposits => new(Db, Rules, new ShardsMapper(Rules));

        private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

        private async Task SetWorkshopAsync(int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            ctx.PlayerSkill.Add(new PlayerSkillDTO { PlayerId = player.Id, SkillType = SkillType.Workshop, Level = 1 });
            await ctx.SaveChangesAsync();
        }

        private async Task UpdateDepositAsync(int id, Action<DepositDTO> change)
        {
            await using var ctx = Db.CreateDbContext();
            var deposit = await ctx.Deposit.SingleAsync(d => d.Id == id);
            change(deposit);
            await ctx.SaveChangesAsync();
        }

        private async Task<DepositDTO> LoadDepositAsync(int id)
        {
            await using var ctx = Db.CreateDbContext();
            return await ctx.Deposit.AsNoTracking().SingleAsync(d => d.Id == id);
        }

        private async Task<PlayerDTO> LoadPlayerAsync(int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            return await ctx.Player.AsNoTracking().SingleAsync(p => p.UserId == uid);
        }

        private async Task SetExploreAsync(int energy, DateTime? last = null, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            player.ExploreEnergy = energy;
            if (last.HasValue) player.LastExploreEnergyUpdateUtc = last.Value;
            await ctx.SaveChangesAsync();
        }

        private async Task<int> RawAsync(OreType ore, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            return await ctx.InventoryItem.Where(i => i.PlayerId == player.Id && i.OreType == ore && i.Stage == ItemStage.Raw)
                .Select(i => i.Quantity).FirstOrDefaultAsync();
        }

        private async Task GiveStagedAsync(OreType ore, ItemStage stage, int quantity, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            ctx.InventoryItem.Add(new InventoryItemDTO { PlayerId = player.Id, OreType = ore, Stage = stage, Quantity = quantity });
            await ctx.SaveChangesAsync();
        }

        private async Task AddMissionAsync(MissionType type, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            ctx.PlayerMission.Add(new PlayerMissionDTO { PlayerId = player.Id, MissionType = type, Status = MissionStatus.Active });
            await ctx.SaveChangesAsync();
        }

        private async Task<PlayerMissionDTO> LoadMissionAsync(MissionType type, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            return await ctx.PlayerMission.AsNoTracking().SingleAsync(m => m.PlayerId == player.Id && m.MissionType == type);
        }

        /// <summary>Cria as ofertas e deixa o slot já aberto, com a defesa derrubada e o estoque dado, pronto para extrair.</summary>
        private async Task<int> SeedBrokenDepositAsync(int slot = 1, int stock = 100, int uid = 7)
        {
            await SeedPlayerAsync(uid);
            await SetWorkshopAsync(uid);
            int id = (await Players.GetStateAsync(uid)).Deposits!.Items.Single(i => i.Slot == slot).Id;
            await UpdateDepositAsync(id, d =>
            {
                d.Status = DepositStatus.Open;
                d.Element = Element.Fire;
                d.DefenseCurrent = 0;
                d.StockMax = stock;
                d.StockRemaining = stock;
            });
            return id;
        }

        private static async Task<ShardsException> ThrowsAsync(Func<Task> action) => await Assert.ThrowsExactlyAsync<ShardsException>(action);

        [TestMethod]
        public async Task Mine_YieldsAmethyst_SpendsEnergyAndLowersTheStock()
        {
            int id = await SeedBrokenDepositAsync();

            var res = await Deposits.MineAsync(7, id, 10);

            Assert.AreEqual(10, res.Units);
            Assert.AreEqual(OreType.Amethyst, res.Drops.Single().OreType);
            Assert.AreEqual(10, res.Drops.Single().Quantity);
            Assert.AreEqual(10, res.XpGained);
            Assert.AreEqual(10, res.EnergyCost);
            Assert.IsFalse(res.Depleted);
            Assert.AreEqual(90, res.Deposit.StockRemaining);
            Assert.IsTrue(res.Deposit.CanMine);
            Assert.AreEqual(Balance.Explore.MaxEnergy - 10, res.Player.ExploreEnergy);
            Assert.AreEqual(10, res.Inventory.Single(i => i.OreType == OreType.Amethyst).Quantity);

            Assert.AreEqual(10, await RawAsync(OreType.Amethyst));
            Assert.AreEqual(Balance.Explore.MaxEnergy - 10, (await LoadPlayerAsync()).ExploreEnergy);
            Assert.AreEqual(90, (await LoadDepositAsync(id)).StockRemaining);
            Assert.AreEqual(DepositStatus.Open, (await LoadDepositAsync(id)).Status);
        }

        [TestMethod]
        public async Task Mine_GivesXp_AndLevelsUp()
        {
            int id = await SeedBrokenDepositAsync();
            await SetExploreAsync(30);

            var res = await Deposits.MineAsync(7, id, 20);

            Assert.AreEqual(20, res.XpGained);
            Assert.IsTrue(res.LevelUp, "20 XP passa dos 15 do nível 1");
            var player = await LoadPlayerAsync();
            Assert.AreEqual(1, player.Level);
            Assert.AreEqual(1, player.SkillPoints);
            Assert.AreEqual(20, player.Experience);
        }

        [TestMethod]
        public async Task Mine_KeepsThePartialRegenerationRemainder()
        {
            int id = await SeedBrokenDepositAsync();
            await SetExploreAsync(10, Start.AddSeconds(-1800));

            var res = await Deposits.MineAsync(7, id, 3);

            Assert.AreEqual(8, res.Player.ExploreEnergy, "10 + 1 regenerado - 3");
            Assert.AreEqual(Start.AddSeconds(-1800 + 1200), (await LoadPlayerAsync()).LastExploreEnergyUpdateUtc);
        }

        [TestMethod]
        public async Task Mine_MoreThanTheStock_ExtractsOnlyWhatIsLeft_AndDepletes()
        {
            Balance.Deposits.MaxOpen = 1;
            int id = await SeedBrokenDepositAsync(stock: 5);
            int medium = (await Players.GetStateAsync(7)).Deposits!.Items.Single(i => i.Slot == 2).Id;
            Assert.IsFalse((await Players.GetStateAsync(7)).Deposits!.Items.Single(i => i.Id == medium).CanAttack, "o limite de abertos está cheio");

            var res = await Deposits.MineAsync(7, id, 20);

            Assert.AreEqual(5, res.Units);
            Assert.AreEqual(5, res.EnergyCost, "só as unidades extraídas custam");
            Assert.IsTrue(res.Depleted);
            Assert.AreEqual(DepositStatus.Depleted, res.Deposit.Status);
            Assert.IsFalse(res.Deposit.CanMine);
            Assert.AreEqual(0, res.Deposit.StockRemaining);
            Assert.AreEqual(Balance.Explore.MaxEnergy - 5, res.Player.ExploreEnergy);
            Assert.AreEqual(0, res.Deposits.OpenCount, "esgotar libera a vaga");
            Assert.IsTrue(res.Deposits.Items.Single(i => i.Id == medium).CanAttack);
        }

        [TestMethod]
        public async Task Mine_WithoutEnoughEnergy_IsRefused_AndChangesNothing()
        {
            int id = await SeedBrokenDepositAsync();
            await SetExploreAsync(4);

            var ex = await ThrowsAsync(() => Deposits.MineAsync(7, id, 10));

            Assert.AreEqual(ShardsErrorCode.NotEnoughExploreEnergy, ex.Code);
            Assert.AreEqual(0, await RawAsync(OreType.Amethyst));
            Assert.AreEqual(4, (await LoadPlayerAsync()).ExploreEnergy);
            Assert.AreEqual(100, (await LoadDepositAsync(id)).StockRemaining);
        }

        [TestMethod]
        public async Task Mine_WhileTheDefenseIsActive_IsRefused()
        {
            await SeedPlayerAsync();
            await SetWorkshopAsync();
            var offers = (await Players.GetStateAsync(7)).Deposits!.Items;

            var sealedOne = await ThrowsAsync(() => Deposits.MineAsync(7, offers[0].Id, 1));
            await UpdateDepositAsync(offers[1].Id, d => { d.Status = DepositStatus.Open; d.DefenseCurrent = 10; });
            var partlyBroken = await ThrowsAsync(() => Deposits.MineAsync(7, offers[1].Id, 1));

            Assert.AreEqual(ShardsErrorCode.DepositDefenseActive, sealedOne.Code);
            Assert.AreEqual(ShardsErrorCode.DepositDefenseActive, partlyBroken.Code);
            Assert.IsFalse(offers[0].CanMine);
        }

        [TestMethod]
        public async Task Mine_ExpiredDepletedForeignOrUnknown_AreRefused()
        {
            int id = await SeedBrokenDepositAsync();
            await SeedPlayerAsync(8);
            int sealedId = (await Players.GetStateAsync(7)).Deposits!.Items.Single(i => i.Slot == 2).Id;

            Assert.AreEqual(ShardsErrorCode.DepositNotFound, (await ThrowsAsync(() => Deposits.MineAsync(8, id, 1))).Code);
            Assert.AreEqual(ShardsErrorCode.DepositNotFound, (await ThrowsAsync(() => Deposits.MineAsync(7, 9999, 1))).Code);

            await UpdateDepositAsync(id, d => { d.Status = DepositStatus.Depleted; d.StockRemaining = 0; });
            Assert.AreEqual(ShardsErrorCode.DepositDepleted, (await ThrowsAsync(() => Deposits.MineAsync(7, id, 1))).Code);

            Time.Advance(TimeSpan.FromDays(7));
            Assert.AreEqual(ShardsErrorCode.DepositExpired, (await ThrowsAsync(() => Deposits.MineAsync(7, sealedId, 1))).Code);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(101)]
        public async Task Mine_InvalidTimes_IsRefused(int times)
        {
            int id = await SeedBrokenDepositAsync();

            var ex = await ThrowsAsync(() => Deposits.MineAsync(7, id, times));

            Assert.AreEqual(ShardsErrorCode.InvalidTimes, ex.Code);
        }

        [TestMethod]
        public async Task Mine_OpenDepositPersistsAcrossWeeks()
        {
            int id = await SeedBrokenDepositAsync();
            Time.Advance(TimeSpan.FromDays(14));

            var res = await Deposits.MineAsync(7, id, 5);

            Assert.AreEqual(5, res.Units);
            Assert.AreEqual(95, res.Deposit.StockRemaining);
        }

        [TestMethod]
        public async Task Mine_RollsTheTierDropTable_PerUnit()
        {
            Balance.Deposits.Tiers[0].Drops =
            [
                new DropOptions { Ore = OreType.Amethyst, Chance = 0.5, Xp = 1 },
                new DropOptions { Ore = OreType.Ruby, Chance = 0.5, Xp = 3 },
            ];
            int id = await SeedBrokenDepositAsync();

            Random.Value = 0.0;
            var first = await Deposits.MineAsync(7, id, 4);
            Assert.AreEqual(4, first.Drops.Single(d => d.OreType == OreType.Amethyst).Quantity);
            Assert.AreEqual(4, first.XpGained);

            Random.Value = 0.9;
            var second = await Deposits.MineAsync(7, id, 4);
            Assert.AreEqual(4, second.Drops.Single(d => d.OreType == OreType.Ruby).Quantity);
            Assert.AreEqual(12, second.XpGained);
            Assert.AreEqual(4, await RawAsync(OreType.Ruby));
        }

        [TestMethod]
        public async Task State_CanMineIsTrueOnlyWhenBrokenAndOpenWithStock()
        {
            int id = await SeedBrokenDepositAsync();
            var state = (await Players.GetStateAsync(7)).Deposits!;

            Assert.AreEqual(1, state.EnergyPerExtraction);
            Assert.IsTrue(state.Items.Single(i => i.Id == id).CanMine);
            Assert.IsTrue(state.Items.Where(i => i.Id != id).All(i => !i.CanMine));
        }

        [TestMethod]
        public async Task Mine_AdvancesTheExtractorMission_AndCompletesIt()
        {
            int id = await SeedBrokenDepositAsync(stock: 400);
            await AddMissionAsync(MissionType.Extractor);

            var first = await Deposits.MineAsync(7, id, 10);
            Assert.AreEqual(10, first.Missions.Single().Progress);
            Assert.AreEqual(MissionType.Extractor, first.Missions.Single().Type);

            for (int i = 0; i < 3; i++)
            {
                await SetExploreAsync(30);
                await Deposits.MineAsync(7, id, 30);
            }

            var mission = await LoadMissionAsync(MissionType.Extractor);
            Assert.AreEqual(100, mission.Progress, "limitado à meta");
            Assert.AreEqual(MissionStatus.Completed, mission.Status);
        }

        [TestMethod]
        public async Task Mine_WithoutTheMissionActive_ChangesNoMission()
        {
            int id = await SeedBrokenDepositAsync();

            var res = await Deposits.MineAsync(7, id, 5);

            Assert.AreEqual(0, res.Missions.Count);
        }

        [TestMethod]
        public async Task Attack_AdvancesFirstAttack_AndVaultOpenedWhenTheDefenseBreaks()
        {
            await SeedPlayerAsync();
            await SetWorkshopAsync();
            int id = (await Players.GetStateAsync(7)).Deposits!.Items.Single(i => i.Slot == 1).Id;
            await UpdateDepositAsync(id, d => { d.Element = Element.Fire; d.DefenseMax = 100; d.DefenseCurrent = 100; });
            await GiveStagedAsync(OreType.Ruby, ItemStage.Faceted, 10);
            await GiveStagedAsync(OreType.Ruby, ItemStage.Polished, 10);
            await AddMissionAsync(MissionType.FirstAttack);
            await AddMissionAsync(MissionType.VaultOpened);

            var req = new AttackDepositReq { Attackers = [new GemStackReq { Ore = "Ruby", Stage = "Polished", Quantity = 2 }] };

            var preview = await Deposits.AttackAsync(7, id, req with { Preview = true });
            Assert.AreEqual(0, preview.Missions.Count, "a prévia não avança missão");
            Assert.AreEqual(MissionStatus.Active, (await LoadMissionAsync(MissionType.FirstAttack)).Status);

            var partial = await Deposits.AttackAsync(7, id, req);
            Assert.AreEqual(MissionType.FirstAttack, partial.Missions.Single().Type);
            Assert.AreEqual(MissionStatus.Completed, (await LoadMissionAsync(MissionType.FirstAttack)).Status);
            Assert.AreEqual(MissionStatus.Active, (await LoadMissionAsync(MissionType.VaultOpened)).Status, "a defesa ainda não zerou");

            var breaking = await Deposits.AttackAsync(7, id, new AttackDepositReq { Attackers = [new GemStackReq { Ore = "Ruby", Stage = "Faceted", Quantity = 10 }] });
            Assert.IsTrue(breaking.Broken);
            Assert.AreEqual(MissionType.VaultOpened, breaking.Missions.Single().Type);
            Assert.AreEqual(MissionStatus.Completed, (await LoadMissionAsync(MissionType.VaultOpened)).Status);
        }

        [TestMethod]
        public void MissionFacts_AttackedAndBrokenDeposits_SatisfyTheConditions()
        {
            DepositDTO Make(DepositStatus status, int current) => new()
            {
                PlayerId = 1, Slot = 1, WeekStart = new DateOnly(2026, 10, 5), Element = Element.Fire,
                DefenseMax = 100, DefenseCurrent = current, StockMax = 10, StockRemaining = 10, Status = status, CreatedAt = Start,
            };

            Assert.IsFalse(MissionTracker.IsSatisfied(MissionType.FirstAttack, new MissionFacts(Deposits: [Make(DepositStatus.Sealed, 100)])));
            Assert.IsTrue(MissionTracker.IsSatisfied(MissionType.FirstAttack, new MissionFacts(Deposits: [Make(DepositStatus.Open, 60)])));
            Assert.IsFalse(MissionTracker.IsSatisfied(MissionType.VaultOpened, new MissionFacts(Deposits: [Make(DepositStatus.Open, 60)])));
            Assert.IsTrue(MissionTracker.IsSatisfied(MissionType.VaultOpened, new MissionFacts(Deposits: [Make(DepositStatus.Open, 0)])));
            Assert.IsFalse(MissionTracker.IsSatisfied(MissionType.Extractor, new MissionFacts(Deposits: [Make(DepositStatus.Depleted, 0)])), "Extrator é de contagem");
        }

        [TestMethod]
        public async Task Attacked_BeforeTheMissionExists_ItIsBornCompleted()
        {
            await SeedPlayerAsync();
            await SetWorkshopAsync();
            int id = (await Players.GetStateAsync(7)).Deposits!.Items[0].Id;
            await UpdateDepositAsync(id, d => { d.Status = DepositStatus.Open; d.DefenseCurrent = 0; });
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == 7);
            ctx.PlayerMission.Add(new PlayerMissionDTO { PlayerId = player.Id, MissionType = MissionType.DoubleLuck, Status = MissionStatus.Claimed });
            await ctx.SaveChangesAsync();

            var state = await Players.GetStateAsync(7);

            Assert.AreEqual(MissionStatus.Completed, state.Missions.Single(m => m.Type == MissionType.FirstAttack).Status);
        }

        [TestMethod]
        public void DailyOreProgress_CountsOnlyCatalogGems_NotTheAmethyst()
        {
            var ores = new Dictionary<OreType, int> { [OreType.Quartz] = 2, [OreType.Ruby] = 3, [OreType.Amethyst] = 5 };

            var progress = DailyTracker.OreProgress(ores, Rules.GemOres).ToDictionary(p => p.Item1, p => p.Item2);

            Assert.AreEqual(2, progress[DailyMissionType.Quartz]);
            Assert.AreEqual(3, progress[DailyMissionType.Gems]);
        }

        [TestMethod]
        public async Task Mine_SerializesTheContractTheFrontExpects()
        {
            int id = await SeedBrokenDepositAsync();

            var res = await Deposits.MineAsync(7, id, 3);
            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(res, Web));
            var root = doc.RootElement;

            Assert.AreEqual(3, root.GetProperty("units").GetInt32());
            Assert.AreEqual("Amethyst", root.GetProperty("drops")[0].GetProperty("oreType").GetString());
            Assert.AreEqual(3, root.GetProperty("drops")[0].GetProperty("quantity").GetInt32());
            Assert.AreEqual(3, root.GetProperty("xpGained").GetInt32());
            Assert.IsFalse(root.GetProperty("levelUp").GetBoolean());
            Assert.AreEqual(3, root.GetProperty("energyCost").GetInt32());
            Assert.IsFalse(root.GetProperty("depleted").GetBoolean());
            Assert.IsTrue(root.GetProperty("deposit").GetProperty("canMine").GetBoolean());
            Assert.AreEqual(Balance.Explore.MaxEnergy - 3, root.GetProperty("player").GetProperty("exploreEnergy").GetInt32());
            Assert.AreEqual(1, root.GetProperty("deposits").GetProperty("energyPerExtraction").GetInt32());
            Assert.AreEqual(JsonValueKind.Array, root.GetProperty("missions").ValueKind);
        }
    }

    [TestClass]
    public class DepositExtractionRulesTests : GameRulesTestBase
    {
        [TestMethod]
        public void AppSettings_Extraction_MatchesTheDecisions()
        {
            Assert.AreEqual(1, Balance.Deposits.EnergyPerExtraction);
            Assert.IsTrue(Balance.Deposits.Tiers.All(t => t.Drops.Single().Xp == 1));
            Assert.IsTrue(Rules.IsCraftable(OreType.Amethyst));
        }

        [TestMethod]
        public void Validator_EnergyPerExtraction_MustBePositive()
        {
            Balance.Deposits.EnergyPerExtraction = 0;

            var failures = new GameBalanceOptionsValidator().Validate(null, Balance).Failures ?? [];

            Assert.IsTrue(failures.Any(f => f.Contains("EnergyPerExtraction")));
        }

        [TestMethod]
        public void Missions_AreInTheCatalog_WithTheDecidedRewards()
        {
            var attack = Shards.Rules.MissionCatalog.Get(MissionType.FirstAttack);
            var vault = Shards.Rules.MissionCatalog.Get(MissionType.VaultOpened);
            var extractor = Shards.Rules.MissionCatalog.Get(MissionType.Extractor);

            Assert.AreEqual(50, attack.Reward.Xp);
            Assert.AreEqual(80, vault.Reward.Xp);
            Assert.AreEqual(500, vault.Reward.Ores.Single().Quantity);
            Assert.AreEqual(100, extractor.Goal);
            Assert.AreEqual(100, extractor.Reward.Xp);
            Assert.AreEqual(1000, extractor.Reward.Ores.Single().Quantity);
        }
    }
}
