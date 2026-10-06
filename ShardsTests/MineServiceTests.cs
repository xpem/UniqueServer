using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shards.Errors;
using Shards.Model.DTO;
using Shards.Service;

namespace ShardsTests
{
    public abstract class ShardsServiceTestBase : GameRulesTestBase
    {
        protected TestDbFactory Db { get; } = new();

        protected PlayerService Players { get; }

        protected MineService Mines { get; }

        protected PlayerSkillService Skills { get; }

        protected PlayerMissionService Missions { get; }

        protected ShardsServiceTestBase()
        {
            var mapper = new ShardsMapper(Rules);
            Players = new PlayerService(Db, Rules, mapper);
            Mines = new MineService(Db, Rules, mapper);
            Skills = new PlayerSkillService(Db, Rules, mapper);
            Missions = new PlayerMissionService(Db, Rules, mapper);
        }

        protected async Task GiveOresAsync(OreType ore, int quantity, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            var item = await ctx.InventoryItem.FirstOrDefaultAsync(i => i.PlayerId == player.Id && i.OreType == ore);
            if (item is null) ctx.InventoryItem.Add(new InventoryItemDTO { PlayerId = player.Id, OreType = ore, Quantity = quantity });
            else item.Quantity += quantity;
            await ctx.SaveChangesAsync();
        }

        /// <summary>Minera 10 vezes (bronze) para completar a Missão 1 e resgata a recompensa. Deixa a Missão 2 ativa.</summary>
        protected async Task CompleteAndClaimFirstMissionAsync(int uid = 7)
        {
            Random.Value = 0.0;
            await Mines.MineAsync(uid, 1, 10);
            await Missions.ClaimAsync(uid, "FirstPickaxe");
        }

        /// <summary>Cria o jogador pelo serviço (Mina 1 e Missão 1) e ajusta a energia.</summary>
        protected async Task<int> SeedPlayerAsync(int uid = 7, int? energy = null, DateTime? lastEnergyUpdate = null)
        {
            await Players.GetStateAsync(uid);

            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            if (energy.HasValue) player.Energy = energy.Value;
            if (lastEnergyUpdate.HasValue) player.LastEnergyUpdateUtc = lastEnergyUpdate.Value;
            await ctx.SaveChangesAsync();

            return player.Id;
        }

        protected async Task<int> InventoryOf(OreType ore, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            return await ctx.InventoryItem.Where(i => i.PlayerId == player.Id && i.OreType == ore).Select(i => i.Quantity).FirstOrDefaultAsync();
        }

        protected async Task<PlayerDTO> PlayerRow(int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            return await ctx.Player.AsNoTracking().SingleAsync(p => p.UserId == uid);
        }
    }

    [TestClass]
    public class MineActiveTests : ShardsServiceTestBase
    {
        [TestMethod]
        public async Task SingleMine_Bronze_SpendsEnergyAndGivesOreAndXp()
        {
            await SeedPlayerAsync();
            Random.Value = 0.0;

            var res = await Mines.MineAsync(7, 1, 1);

            Assert.AreEqual(OreType.Bronze, res.Drops.Single().OreType);
            Assert.AreEqual(1, res.Drops.Single().Quantity);
            Assert.AreEqual(1, res.XpGained);
            Assert.IsFalse(res.LevelUp);
            Assert.AreEqual(99, res.Player.Energy);
            Assert.AreEqual(Start.AddSeconds(216), res.Player.NextEnergyAtUtc, "gastar com a energia cheia recomeça a contagem agora");
            Assert.AreEqual(1, await InventoryOf(OreType.Bronze));
        }

        [TestMethod]
        public async Task SingleMine_Silver_GivesThreeXp()
        {
            await SeedPlayerAsync();
            Random.Value = 0.9;

            var res = await Mines.MineAsync(7, 1, 1);

            Assert.AreEqual(OreType.Silver, res.Drops.Single().OreType);
            Assert.AreEqual(3, res.XpGained);
            Assert.AreEqual(1, await InventoryOf(OreType.Silver));
        }

        [TestMethod]
        public async Task Times_SpendsOneEnergyPerMining_AndAggregatesDrops()
        {
            await SeedPlayerAsync();
            Random.Value = 0.0;

            var res = await Mines.MineAsync(7, 1, 10);

            Assert.AreEqual(10, res.Drops.Single().Quantity);
            Assert.AreEqual(10, res.XpGained);
            Assert.AreEqual(90, res.Player.Energy);
            Assert.AreEqual(10, await InventoryOf(OreType.Bronze));
        }

        [TestMethod]
        public async Task Times_CanSpendAllEnergyAtOnce()
        {
            await SeedPlayerAsync();
            Random.Value = 0.0;

            var res = await Mines.MineAsync(7, 1, 100);

            Assert.AreEqual(0, res.Player.Energy);
            Assert.AreEqual(100, await InventoryOf(OreType.Bronze));
        }

        [TestMethod]
        public async Task LevelUp_GivesSkillPoint()
        {
            await SeedPlayerAsync();
            Random.Value = 0.0;

            var res = await Mines.MineAsync(7, 1, 15); // 15 XP = nível 1

            Assert.IsTrue(res.LevelUp);
            Assert.AreEqual(1, res.Player.Level);
            Assert.AreEqual(1, res.Player.SkillPoints);
            Assert.AreEqual(30, res.Player.ExperienceToNextLevel);
        }

        [TestMethod]
        public async Task LevelUp_CanGainSeveralLevelsAtOnce()
        {
            await SeedPlayerAsync();
            Random.Value = 0.9; // só prata: 3 XP cada

            var res = await Mines.MineAsync(7, 1, 30); // 90 XP = nível 3

            Assert.AreEqual(90, res.XpGained);
            Assert.AreEqual(3, res.Player.Level);
            Assert.AreEqual(3, res.Player.SkillPoints);
        }

        [TestMethod]
        public async Task NotEnoughEnergy_FailsWithoutChangingAnything()
        {
            await SeedPlayerAsync(energy: 5, lastEnergyUpdate: Start);

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.MineAsync(7, 1, 10));

            Assert.AreEqual(ShardsErrorCode.NotEnoughEnergy, ex.Code);
            var player = await PlayerRow();
            Assert.AreEqual(5, player.Energy);
            Assert.AreEqual(0, player.Experience);
            Assert.AreEqual(0, await InventoryOf(OreType.Bronze));
        }

        [TestMethod]
        public async Task Energy_RegeneratedBeforeSpending_KeepsTheRemainder()
        {
            await SeedPlayerAsync(energy: 50, lastEnergyUpdate: Start);
            Time.Advance(TimeSpan.FromSeconds(500)); // 52 pontos + 68 s de resto
            Random.Value = 0.0;

            var res = await Mines.MineAsync(7, 1, 1);

            Assert.AreEqual(51, res.Player.Energy);
            Assert.AreEqual(Start.AddSeconds(432), (await PlayerRow()).LastEnergyUpdateUtc);
        }

        [TestMethod]
        public async Task Energy_RegeneratedPastTheCost_AllowsMining()
        {
            await SeedPlayerAsync(energy: 0, lastEnergyUpdate: Start);
            Time.Advance(TimeSpan.FromSeconds(216 * 3));
            Random.Value = 0.0;

            var res = await Mines.MineAsync(7, 1, 3);

            Assert.AreEqual(0, res.Player.Energy);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(101)]
        public async Task InvalidTimes_IsRejectedBeforeAnything(int times)
        {
            await SeedPlayerAsync();

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.MineAsync(7, 1, times));

            Assert.AreEqual(ShardsErrorCode.InvalidTimes, ex.Code);
            Assert.AreEqual(100, (await PlayerRow()).Energy);
        }

        [TestMethod]
        public async Task UnknownPlayer_IsPlayerNotFound()
        {
            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.MineAsync(99, 1, 1));

            Assert.AreEqual(ShardsErrorCode.PlayerNotFound, ex.Code);
        }

        [TestMethod]
        public async Task MineNotOwned_IsMineNotFound()
        {
            await SeedPlayerAsync();

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.MineAsync(7, 2, 1));

            Assert.AreEqual(ShardsErrorCode.MineNotFound, ex.Code);
            Assert.AreEqual(100, (await PlayerRow()).Energy);
        }

        [TestMethod]
        public async Task Inventory_AccumulatesAcrossCalls()
        {
            await SeedPlayerAsync();
            Random.Value = 0.0;

            await Mines.MineAsync(7, 1, 3);
            await Mines.MineAsync(7, 1, 4);

            Assert.AreEqual(7, await InventoryOf(OreType.Bronze));
            await using var ctx = Db.CreateDbContext();
            Assert.AreEqual(1, await ctx.InventoryItem.CountAsync(), "uma linha por tipo de minério");
        }

        [TestMethod]
        public async Task DoubleDrop_GivesExtraOreForTheSameEnergy()
        {
            int playerId = await SeedPlayerAsync();
            await AddSkillAsync(playerId, level: 1); // 15%
            Random.Value = 0.10;                     // < 0,15: aciona; bronze

            var res = await Mines.MineAsync(7, 1, 1);

            Assert.AreEqual(2, res.Drops.Single().Quantity);
            Assert.AreEqual(1, res.DoubleDrops);
            Assert.AreEqual(1, res.XpGained, "o XP é o do drop, uma vez por mineração");
            Assert.AreEqual(99, res.Player.Energy);
        }

        [TestMethod]
        public async Task DoubleDrop_NotTriggered_WhenRollIsAboveTheChance()
        {
            int playerId = await SeedPlayerAsync();
            await AddSkillAsync(playerId, level: 1);
            Random.Value = 0.20;

            var res = await Mines.MineAsync(7, 1, 1);

            Assert.AreEqual(1, res.Drops.Single().Quantity);
            Assert.AreEqual(0, res.DoubleDrops);
        }

        [TestMethod]
        public async Task WithoutTheSkill_NeverDoubles()
        {
            await SeedPlayerAsync();
            Random.Value = 0.0;

            var res = await Mines.MineAsync(7, 1, 20);

            Assert.AreEqual(0, res.DoubleDrops);
            Assert.AreEqual(20, res.Drops.Single().Quantity);
        }

        [TestMethod]
        public async Task FirstMission_ProgressesByEnergySpent_AndCompletesAtTen()
        {
            await SeedPlayerAsync();
            Random.Value = 0.0;

            var partial = await Mines.MineAsync(7, 1, 4);
            var mission = partial.Missions.Single();
            Assert.AreEqual(MissionType.FirstPickaxe, mission.Type);
            Assert.AreEqual(4, mission.Progress);
            Assert.AreEqual(MissionStatus.Active, mission.Status);

            var done = await Mines.MineAsync(7, 1, 6);
            mission = done.Missions.Single();
            Assert.AreEqual(10, mission.Progress);
            Assert.AreEqual(MissionStatus.Completed, mission.Status);

            await using var ctx = Db.CreateDbContext();
            Assert.IsNotNull((await ctx.PlayerMission.SingleAsync()).CompletedAt);
        }

        [TestMethod]
        public async Task FirstMission_ProgressIsCappedAtTheGoal_AndStaysCompleted()
        {
            await SeedPlayerAsync();
            Random.Value = 0.0;

            var res = await Mines.MineAsync(7, 1, 25);
            Assert.AreEqual(10, res.Missions.Single().Progress);

            var again = await Mines.MineAsync(7, 1, 5);
            Assert.AreEqual(0, again.Missions.Count, "missão concluída não muda mais");
        }

        private async Task AddSkillAsync(int playerId, int level)
        {
            await using var ctx = Db.CreateDbContext();
            ctx.PlayerSkill.Add(new PlayerSkillDTO { PlayerId = playerId, SkillType = SkillType.DoubleDrop, Level = level });
            await ctx.SaveChangesAsync();
        }
    }

    [TestClass]
    public class CollectCartTests : ShardsServiceTestBase
    {
        [TestMethod]
        public async Task EmptyCart_IsCartEmpty()
        {
            await SeedPlayerAsync();

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.CollectCartAsync(7, 1));

            Assert.AreEqual(ShardsErrorCode.CartEmpty, ex.Code);
        }

        [TestMethod]
        public async Task Collect_MovesOresToInventory_AndResetsTheCart()
        {
            await SeedPlayerAsync();
            Time.Advance(TimeSpan.FromMinutes(10)); // 3 minérios
            Random.Value = 0.0;

            var res = await Mines.CollectCartAsync(7, 1);

            Assert.AreEqual(3, res.Total);
            Assert.AreEqual(OreType.Bronze, res.Collected.Single().OreType);
            Assert.AreEqual(3, res.Inventory.Single().Quantity);
            Assert.AreEqual(0, res.Cart.CartAmount);

            await using var ctx = Db.CreateDbContext();
            Assert.AreEqual(Time.Now.UtcDateTime, (await ctx.Mine.SingleAsync()).LastCartCollectionUtc);
        }

        [TestMethod]
        public async Task Collect_Twice_SecondIsEmpty()
        {
            await SeedPlayerAsync();
            Time.Advance(TimeSpan.FromMinutes(10));

            await Mines.CollectCartAsync(7, 1);
            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.CollectCartAsync(7, 1));

            Assert.AreEqual(ShardsErrorCode.CartEmpty, ex.Code);
        }

        [TestMethod]
        public async Task Collect_IsCappedAtTheCartCapacity()
        {
            await SeedPlayerAsync();
            Time.Advance(TimeSpan.FromDays(3));

            var res = await Mines.CollectCartAsync(7, 1);

            Assert.AreEqual(100, res.Total);
        }

        [TestMethod]
        public async Task Collect_RollsTheDropTable()
        {
            await SeedPlayerAsync();
            Time.Advance(TimeSpan.FromMinutes(10));
            Random.Value = 0.9;

            var res = await Mines.CollectCartAsync(7, 1);

            Assert.AreEqual(OreType.Silver, res.Collected.Single().OreType);
            Assert.AreEqual(3, await InventoryOf(OreType.Silver));
        }

        [TestMethod]
        public async Task Collect_GivesNoXp_AndDoesNotTouchEnergy()
        {
            await SeedPlayerAsync();
            Time.Advance(TimeSpan.FromMinutes(10));

            await Mines.CollectCartAsync(7, 1);

            var player = await PlayerRow();
            Assert.AreEqual(0, player.Experience);
            Assert.AreEqual(100, player.Energy);
        }

        [TestMethod]
        public async Task Collect_AddsToExistingInventory()
        {
            await SeedPlayerAsync();
            Random.Value = 0.0;
            await Mines.MineAsync(7, 1, 5);
            Time.Advance(TimeSpan.FromMinutes(10));

            await Mines.CollectCartAsync(7, 1);

            Assert.AreEqual(8, await InventoryOf(OreType.Bronze));
        }

        [TestMethod]
        public async Task UnknownPlayer_AndMineNotOwned_AreRejected()
        {
            var noPlayer = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.CollectCartAsync(99, 1));
            Assert.AreEqual(ShardsErrorCode.PlayerNotFound, noPlayer.Code);

            await SeedPlayerAsync();
            var noMine = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.CollectCartAsync(7, 2));
            Assert.AreEqual(ShardsErrorCode.MineNotFound, noMine.Code);
        }
    }

    [TestClass]
    public class ConcurrencyRetryUniqueViolationTests
    {
        private static DbUpdateException UniqueViolation() =>
            new("duplicado", new PostgresException("duplicate key", "ERROR", "ERROR", PostgresErrorCodes.UniqueViolation));

        [TestMethod]
        public async Task UniqueViolation_IsRetried()
        {
            int calls = 0;

            int result = await ConcurrencyRetry.RunAsync(() =>
            {
                calls++;
                if (calls == 1) throw UniqueViolation();
                return Task.FromResult(1);
            });

            Assert.AreEqual(1, result);
            Assert.AreEqual(2, calls);
        }

        [TestMethod]
        public async Task OtherDatabaseErrors_AreNotRetried()
        {
            int calls = 0;
            var other = new DbUpdateException("fk", new PostgresException("fk", "ERROR", "ERROR", PostgresErrorCodes.ForeignKeyViolation));

            await Assert.ThrowsExactlyAsync<DbUpdateException>(() => ConcurrencyRetry.RunAsync<int>(() =>
            {
                calls++;
                throw other;
            }));

            Assert.AreEqual(1, calls);
        }
    }
}
