using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shards.Errors;
using Shards.Model.DTO;
using Shards.Repo;
using Shards.Service;

namespace ShardsTests
{
    /// <summary>DbContext em memória, um banco novo por teste. Ignora o aviso de transação (o provider em memória não as suporta).</summary>
    public class TestDbFactory : IDbContextFactory<ShardsDbctx>
    {
        private readonly DbContextOptions<ShardsDbctx> _options = new DbContextOptionsBuilder<ShardsDbctx>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        public ShardsDbctx CreateDbContext() => new(_options);
    }

    [TestClass]
    public class PlayerServiceStateTests : GameRulesTestBase
    {
        private readonly TestDbFactory _db = new();
        private readonly PlayerService _service;

        public PlayerServiceStateTests()
        {
            _service = new PlayerService(_db, Rules, new ShardsMapper(Rules));
        }

        [TestMethod]
        public async Task FirstAccess_CreatesPlayerMineAndFirstMission()
        {
            var state = await _service.GetStateAsync(7);

            Assert.AreEqual(Start, state.ServerTimeUtc);

            Assert.AreEqual(0, state.Player.Level);
            Assert.AreEqual(0, state.Player.Experience);
            Assert.AreEqual(15, state.Player.ExperienceToNextLevel);
            Assert.AreEqual(0, state.Player.SkillPoints);
            Assert.AreEqual(100, state.Player.Energy);
            Assert.AreEqual(100, state.Player.MaxEnergy);
            Assert.IsNull(state.Player.NextEnergyAtUtc);

            Assert.AreEqual(0, state.Inventory.Count);

            var mine = state.Mines.Single();
            Assert.AreEqual(1, mine.MineNumber);
            Assert.AreEqual(100, mine.CartCapacity);
            Assert.AreEqual(0, mine.CartAmount);
            Assert.AreEqual(0.33, mine.OresPerMinute, 1e-9);
            Assert.IsNotNull(mine.CartFullAtUtc);

            var mission = state.Missions.Single();
            Assert.AreEqual(MissionType.FirstPickaxe, mission.Type);
            Assert.AreEqual(MissionStatus.Active, mission.Status);
            Assert.AreEqual(0, mission.Progress);
            Assert.AreEqual(10, mission.Goal);
            Assert.AreEqual(15, mission.Reward.Xp);
        }

        [TestMethod]
        public async Task FirstAccess_SkillsListsAllSkillsAtLevelZero()
        {
            var state = await _service.GetStateAsync(7);

            var skill = state.Skills.Single();
            Assert.AreEqual(SkillType.DoubleDrop, skill.Type);
            Assert.AreEqual(0, skill.Level);
            Assert.AreEqual(5, skill.MaxLevel);
        }

        [TestMethod]
        public async Task FirstAccess_NextMineIsMineTwoAndNotAffordable()
        {
            var state = await _service.GetStateAsync(7);

            Assert.IsNotNull(state.NextMine);
            Assert.AreEqual(2, state.NextMine.MineNumber);
            Assert.IsFalse(state.NextMine.CanAfford);
            Assert.AreEqual(150, state.NextMine.Cost.Single(c => c.OreType == OreType.Bronze).Quantity);
            Assert.AreEqual(30, state.NextMine.Cost.Single(c => c.OreType == OreType.Silver).Quantity);
        }

        [TestMethod]
        public async Task SecondAccess_DoesNotDuplicateAnything()
        {
            await _service.GetStateAsync(7);
            await _service.GetStateAsync(7);

            await using var ctx = _db.CreateDbContext();
            Assert.AreEqual(1, await ctx.Player.CountAsync());
            Assert.AreEqual(1, await ctx.Mine.CountAsync());
            Assert.AreEqual(1, await ctx.PlayerMission.CountAsync());
        }

        [TestMethod]
        public async Task DifferentUsers_GetSeparatePlayers()
        {
            await _service.GetStateAsync(1);
            await _service.GetStateAsync(2);

            await using var ctx = _db.CreateDbContext();
            Assert.AreEqual(2, await ctx.Player.CountAsync());
            Assert.AreEqual(2, await ctx.Mine.CountAsync());
            Assert.AreEqual(1, await ctx.Player.CountAsync(p => p.UserId == 1));
        }

        [TestMethod]
        public async Task Energy_IsRegeneratedAndPersisted_KeepingTheRemainder()
        {
            await SeedPlayerAsync(uid: 7, energy: 50, lastEnergyUpdate: Start);
            Time.Advance(TimeSpan.FromSeconds(500)); // 2 pontos + 68 s de resto

            var state = await _service.GetStateAsync(7);

            Assert.AreEqual(52, state.Player.Energy);
            Assert.AreEqual(Start.AddSeconds(432 + 216), state.Player.NextEnergyAtUtc);

            await using var ctx = _db.CreateDbContext();
            var player = await ctx.Player.SingleAsync();
            Assert.AreEqual(52, player.Energy);
            Assert.AreEqual(Start.AddSeconds(432), player.LastEnergyUpdateUtc);
        }

        [TestMethod]
        public async Task Energy_FullAndUntouched_IsNotRewritten()
        {
            await _service.GetStateAsync(7);
            DateTime createdUpdatedAt;
            await using (var ctx = _db.CreateDbContext())
                createdUpdatedAt = (await ctx.Player.SingleAsync()).UpdatedAt;

            Time.Advance(TimeSpan.FromHours(3));
            await _service.GetStateAsync(7);

            await using var after = _db.CreateDbContext();
            Assert.AreEqual(createdUpdatedAt, (await after.Player.SingleAsync()).UpdatedAt);
        }

        [TestMethod]
        public async Task Cart_ShowsAccumulatedOresWithoutChangingTheDatabase()
        {
            await _service.GetStateAsync(7);
            Time.Advance(TimeSpan.FromMinutes(10)); // 3,3 -> 3

            var state = await _service.GetStateAsync(7);

            Assert.AreEqual(3, state.Mines.Single().CartAmount);

            await using var ctx = _db.CreateDbContext();
            Assert.AreEqual(Start, (await ctx.Mine.SingleAsync()).LastCartCollectionUtc);
        }

        [TestMethod]
        public async Task NextMine_CanAfford_WhenInventoryCoversTheCost()
        {
            int playerId = await SeedPlayerAsync(uid: 7, energy: 100, lastEnergyUpdate: Start);
            await using (var ctx = _db.CreateDbContext())
            {
                ctx.InventoryItem.Add(new InventoryItemDTO { PlayerId = playerId, OreType = OreType.Bronze, Quantity = 150 });
                ctx.InventoryItem.Add(new InventoryItemDTO { PlayerId = playerId, OreType = OreType.Silver, Quantity = 30 });
                await ctx.SaveChangesAsync();
            }

            var state = await _service.GetStateAsync(7);

            Assert.IsTrue(state.NextMine!.CanAfford);
            Assert.AreEqual(2, state.Inventory.Count);
        }

        [TestMethod]
        public async Task NextMine_NotAffordable_WhenOneOreIsShort()
        {
            int playerId = await SeedPlayerAsync(uid: 7, energy: 100, lastEnergyUpdate: Start);
            await using (var ctx = _db.CreateDbContext())
            {
                ctx.InventoryItem.Add(new InventoryItemDTO { PlayerId = playerId, OreType = OreType.Bronze, Quantity = 150 });
                ctx.InventoryItem.Add(new InventoryItemDTO { PlayerId = playerId, OreType = OreType.Silver, Quantity = 29 });
                await ctx.SaveChangesAsync();
            }

            Assert.IsFalse((await _service.GetStateAsync(7)).NextMine!.CanAfford);
        }

        [TestMethod]
        public async Task NextMine_IsNull_WhenAllConfiguredMinesAreOwned()
        {
            int playerId = await SeedPlayerAsync(uid: 7, energy: 100, lastEnergyUpdate: Start);
            await using (var ctx = _db.CreateDbContext())
            {
                ctx.Mine.Add(new MineDTO { PlayerId = playerId, MineNumber = 2, CreatedAt = Start, LastCartCollectionUtc = Start });
                await ctx.SaveChangesAsync();
            }

            var state = await _service.GetStateAsync(7);

            Assert.IsNull(state.NextMine);
            Assert.AreEqual(2, state.Mines.Count);
            CollectionAssert.AreEqual(new[] { 1, 2 }, state.Mines.Select(m => m.MineNumber).ToArray());
        }

        [TestMethod]
        public async Task Missions_ClaimedAreHidden_CompletedAreVisible()
        {
            int playerId = await SeedPlayerAsync(uid: 7, energy: 100, lastEnergyUpdate: Start);
            await using (var ctx = _db.CreateDbContext())
            {
                var first = await ctx.PlayerMission.SingleAsync();
                first.Status = MissionStatus.Claimed;
                ctx.PlayerMission.Add(new PlayerMissionDTO
                {
                    PlayerId = playerId,
                    MissionType = MissionType.Specialization,
                    Status = MissionStatus.Completed,
                    Progress = 1,
                });
                await ctx.SaveChangesAsync();
            }

            var mission = (await _service.GetStateAsync(7)).Missions.Single();

            Assert.AreEqual(MissionType.Specialization, mission.Type);
            Assert.AreEqual(MissionStatus.Completed, mission.Status);
            Assert.AreEqual(20, mission.Reward.Ores.Single(o => o.OreType == OreType.Bronze).Quantity);
        }

        [TestMethod]
        public async Task Inventory_ZeroQuantityRowsAreOmitted()
        {
            int playerId = await SeedPlayerAsync(uid: 7, energy: 100, lastEnergyUpdate: Start);
            await using (var ctx = _db.CreateDbContext())
            {
                ctx.InventoryItem.Add(new InventoryItemDTO { PlayerId = playerId, OreType = OreType.Gold, Quantity = 0 });
                ctx.InventoryItem.Add(new InventoryItemDTO { PlayerId = playerId, OreType = OreType.Silver, Quantity = 4 });
                await ctx.SaveChangesAsync();
            }

            var inventory = (await _service.GetStateAsync(7)).Inventory;

            Assert.AreEqual(OreType.Silver, inventory.Single().OreType);
        }

        /// <summary>Cria o jogador pelo próprio serviço (Mina 1 e Missão 1) e ajusta a energia, devolvendo o Id.</summary>
        private async Task<int> SeedPlayerAsync(int uid, int energy, DateTime lastEnergyUpdate)
        {
            await _service.GetStateAsync(uid);

            await using var ctx = _db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            player.Energy = energy;
            player.LastEnergyUpdateUtc = lastEnergyUpdate;
            await ctx.SaveChangesAsync();

            return player.Id;
        }
    }

    [TestClass]
    public class ConcurrencyRetryTests
    {
        [TestMethod]
        public async Task RetriesAfterConflict_ThenSucceeds()
        {
            int calls = 0;

            int result = await ConcurrencyRetry.RunAsync(() =>
            {
                calls++;
                if (calls < 3) throw new DbUpdateConcurrencyException();
                return Task.FromResult(42);
            });

            Assert.AreEqual(42, result);
            Assert.AreEqual(3, calls);
        }

        [TestMethod]
        public async Task GivesUp_WithConcurrencyConflict()
        {
            int calls = 0;

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => ConcurrencyRetry.RunAsync<int>(() =>
            {
                calls++;
                throw new DbUpdateConcurrencyException();
            }));

            Assert.AreEqual(ShardsErrorCode.ConcurrencyConflict, ex.Code);
            Assert.AreEqual(ConcurrencyRetry.MaxAttempts, calls);
        }

        [TestMethod]
        public async Task OtherExceptions_AreNotRetried()
        {
            int calls = 0;

            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => ConcurrencyRetry.RunAsync<int>(() =>
            {
                calls++;
                throw new InvalidOperationException();
            }));

            Assert.AreEqual(1, calls);
        }
    }
}
