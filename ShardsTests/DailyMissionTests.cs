using Microsoft.EntityFrameworkCore;
using Shards.Errors;
using Shards.Model.DTO;
using Shards.Rules;
using Shards.Service;

namespace ShardsTests
{
    [TestClass]
    public class DailyRulesTests : GameRulesTestBase
    {
        [TestMethod]
        public void Day_TurnsAtTheResetHour_NotAtUtcMidnight()
        {
            Time.Now = new DateTimeOffset(2026, 10, 10, 2, 59, 0, TimeSpan.Zero);
            Assert.AreEqual(new DateOnly(2026, 10, 9), Rules.DailyDay, "02:59 UTC ainda é o dia anterior (reset 03:00 UTC)");

            Time.Now = new DateTimeOffset(2026, 10, 10, 3, 0, 0, TimeSpan.Zero);
            Assert.AreEqual(new DateOnly(2026, 10, 10), Rules.DailyDay);

            Time.Now = new DateTimeOffset(2026, 10, 10, 23, 59, 0, TimeSpan.Zero);
            Assert.AreEqual(new DateOnly(2026, 10, 10), Rules.DailyDay);
        }

        [TestMethod]
        public void NextReset_IsTheNextResetHourInUtc()
        {
            Time.Now = new DateTimeOffset(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

            Assert.AreEqual(new DateTime(2026, 10, 11, 3, 0, 0, DateTimeKind.Utc), Rules.NextDailyResetUtc);
            Assert.AreEqual(DateTimeKind.Utc, Rules.NextDailyResetUtc.Kind);
        }

        [TestMethod]
        public void ChestReward_GrowsPerStreakDay_AndDoublesOnTheLastDayOfTheCycle()
        {
            Assert.AreEqual((10, 100), Rules.ChestReward(1));
            Assert.AreEqual((10, 120), Rules.ChestReward(2));
            Assert.AreEqual((10, 200), Rules.ChestReward(6));
            Assert.AreEqual((10, 440), Rules.ChestReward(7), "(100 + 6 * 20) * 2");
            Assert.AreEqual((10, 100), Rules.ChestReward(8), "o ciclo recomeça");
        }

        [TestMethod]
        public void Catalog_HasEveryType_WithPositiveGoals_AndOnlyQuartzRewards()
        {
            foreach (DailyMissionType type in Enum.GetValues<DailyMissionType>())
            {
                var def = DailyMissionCatalog.Get(type);
                Assert.IsTrue(def.Goal > 0, type.ToString());
                Assert.IsTrue(def.Reward.Ores.All(o => o.Ore == OreType.Quartz), $"{type} não deve pagar gemas");
            }
        }

        [TestMethod]
        public void AppSettings_Daily_MatchesThePlan()
        {
            Assert.AreEqual(3, Balance.Daily.ResetHourUtc);
            Assert.AreEqual(3, Balance.Daily.MissionsPerDay);
            Assert.AreEqual(7, Balance.Daily.Streak.CycleDays);
            Assert.IsTrue(new Shards.Config.GameBalanceOptionsValidator().Validate(null, Balance).Succeeded);
        }

        [TestMethod]
        public void Validator_RejectsBrokenDailyConfig()
        {
            Balance.Daily.ResetHourUtc = 24;
            Balance.Daily.Streak.CycleDays = 0;

            var result = new Shards.Config.GameBalanceOptionsValidator().Validate(null, Balance);

            Assert.IsTrue(result.Failed);
            Assert.IsTrue(result.Failures.Any(f => f.Contains("ResetHourUtc")));
            Assert.IsTrue(result.Failures.Any(f => f.Contains("CycleDays")));
        }
    }

    [TestClass]
    public class DailyTrackerTests
    {
        private static readonly DateOnly Day = new(2026, 10, 10);
        private static readonly DateTime Now = new(2026, 10, 10, 12, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        public void Create_AlwaysIncludesLoginCompleted_AndFillsTheRest()
        {
            var created = DailyTracker.CreateMissingToday([], 5, Day, 3, new DailyFacts(true, 2), Now);

            Assert.AreEqual(3, created.Count);
            Assert.AreEqual(3, created.Select(c => c.DailyType).Distinct().Count());
            var login = created.Single(c => c.DailyType == DailyMissionType.Login);
            Assert.AreEqual(MissionStatus.Completed, login.Status);
            Assert.IsTrue(created.Where(c => c.DailyType != DailyMissionType.Login).All(c => c.Status == MissionStatus.Active && c.Progress == 0));
        }

        [TestMethod]
        public void Create_IsDeterministicPerPlayerAndDay()
        {
            var facts = new DailyFacts(true, 2);

            var a = DailyTracker.CreateMissingToday([], 5, Day, 3, facts, Now).Select(c => c.DailyType).OrderBy(t => t).ToArray();
            var b = DailyTracker.CreateMissingToday([], 5, Day, 3, facts, Now.AddHours(3)).Select(c => c.DailyType).OrderBy(t => t).ToArray();

            CollectionAssert.AreEqual(a, b);
        }

        [TestMethod]
        public void Create_VariesAcrossDaysAndPlayers()
        {
            var facts = new DailyFacts(true, 2);
            var sets = new HashSet<string>();

            for (int i = 0; i < 30; i++)
                sets.Add(string.Join(",", DailyTracker.CreateMissingToday([], 5, Day.AddDays(i), 3, facts, Now).Select(c => c.DailyType).OrderBy(t => t)));

            Assert.IsTrue(sets.Count > 1, "30 dias não podem dar sempre o mesmo conjunto");
        }

        [TestMethod]
        public void Create_RespectsEligibility()
        {
            for (int player = 1; player <= 50; player++)
            {
                var none = DailyTracker.CreateMissingToday([], player, Day, 7, new DailyFacts(false, 0), Now).Select(c => c.DailyType).ToArray();
                CollectionAssert.DoesNotContain(none, DailyMissionType.Gems);
                CollectionAssert.DoesNotContain(none, DailyMissionType.Polish);
                CollectionAssert.DoesNotContain(none, DailyMissionType.Facet);

                var level1 = DailyTracker.CreateMissingToday([], player, Day, 7, new DailyFacts(true, 1), Now).Select(c => c.DailyType).ToArray();
                CollectionAssert.Contains(level1, DailyMissionType.Polish);
                CollectionAssert.DoesNotContain(level1, DailyMissionType.Facet, "Facet exige Workshop nível 2");
            }
        }

        [TestMethod]
        public void Create_DoesNothing_WhenTodayAlreadyHasMissions()
        {
            var existing = new List<DailyMissionDTO> { new() { PlayerId = 5, Day = Day, DailyType = DailyMissionType.Cart } };

            Assert.AreEqual(0, DailyTracker.CreateMissingToday(existing, 5, Day, 3, new DailyFacts(true, 2), Now).Count);
        }

        [TestMethod]
        public void Advance_CapsAtTheGoal_AndCompletes_AndIgnoresCompleted()
        {
            var mission = new DailyMissionDTO { PlayerId = 5, Day = Day, DailyType = DailyMissionType.Energy };

            var first = DailyTracker.AdvanceChanged([mission], DailyMissionType.Energy, 30, Now);
            Assert.AreEqual(1, first.Count);
            Assert.AreEqual(MissionStatus.Active, mission.Status);

            DailyTracker.AdvanceChanged([mission], DailyMissionType.Energy, 100, Now);
            Assert.AreEqual(50, mission.Progress);
            Assert.AreEqual(MissionStatus.Completed, mission.Status);

            Assert.AreEqual(0, DailyTracker.AdvanceChanged([mission], DailyMissionType.Energy, 5, Now).Count, "concluída não muda mais");
        }

        [TestMethod]
        public void Advance_IgnoresZeroAndOtherTypes()
        {
            var mission = new DailyMissionDTO { PlayerId = 5, Day = Day, DailyType = DailyMissionType.Cart };

            Assert.AreEqual(0, DailyTracker.AdvanceChanged([mission], DailyMissionType.Cart, 0, Now).Count);
            Assert.AreEqual(0, DailyTracker.AdvanceChanged([mission], DailyMissionType.Energy, 5, Now).Count);
        }
    }

    [TestClass]
    public class DailyServiceTests : ShardsServiceTestBase
    {
        private DailyService Daily => new(Db, Rules, new ShardsMapper(Rules));

        private WorkshopService Workshop => new(Db, Rules, new ShardsMapper(Rules));

        /// <summary>Jogador com a Missão 1 resgatada (as diárias liberadas) e o dia já criado.</summary>
        private async Task UnlockedPlayerAsync()
        {
            await SeedPlayerAsync();
            await CompleteAndClaimFirstMissionAsync();
        }

        /// <summary>Troca as diárias de hoje por Active das informadas (atalho para controlar o cenário).</summary>
        private async Task SetTodayAsync(params DailyMissionType[] types)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync();
            DateOnly day = Rules.DailyDay;

            ctx.DailyMission.RemoveRange(await ctx.DailyMission.Where(d => d.PlayerId == player.Id && d.Day == day).ToListAsync());
            foreach (var type in types)
                ctx.DailyMission.Add(new DailyMissionDTO { PlayerId = player.Id, Day = day, DailyType = type });
            await ctx.SaveChangesAsync();
        }

        private async Task<DailyMissionDTO> RowAsync(DailyMissionType type)
        {
            await using var ctx = Db.CreateDbContext();
            return await ctx.DailyMission.AsNoTracking().SingleAsync(d => d.Day == Rules.DailyDay && d.DailyType == type);
        }

        private async Task SetSkillAsync(SkillType type, int level)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync();
            ctx.PlayerSkill.Add(new PlayerSkillDTO { PlayerId = player.Id, SkillType = type, Level = level });
            await ctx.SaveChangesAsync();
        }

        private async Task SetProductionAsync(OreType production)
        {
            await using var ctx = Db.CreateDbContext();
            (await ctx.Mine.SingleAsync(m => m.MineNumber == 1)).ProductionType = production;
            await ctx.SaveChangesAsync();
        }

        /// <summary>Cria as diárias do dia, conclui todas e resgata uma a uma. Deixa o baú pronto.</summary>
        private async Task ClaimAllTodayAsync()
        {
            var state = await Players.GetStateAsync(7);
            await using (var ctx = Db.CreateDbContext())
            {
                foreach (var row in await ctx.DailyMission.Where(d => d.Day == Rules.DailyDay && d.Status == MissionStatus.Active).ToListAsync())
                    row.Status = MissionStatus.Completed;
                await ctx.SaveChangesAsync();
            }

            foreach (var mission in state.Daily!.Missions)
                await Daily.ClaimAsync(7, mission.Type.ToString());
        }

        // ---------------------------------------------------------------- state

        [TestMethod]
        public async Task State_HasNoDaily_UntilTheFirstMissionIsClaimed()
        {
            await SeedPlayerAsync();

            Assert.IsNull((await Players.GetStateAsync(7)).Daily);
        }

        [TestMethod]
        public async Task State_CreatesTheDay_WithLoginCompleted_AndIsStableOnReread()
        {
            await UnlockedPlayerAsync();

            var first = (await Players.GetStateAsync(7)).Daily!;
            var second = (await Players.GetStateAsync(7)).Daily!;

            Assert.AreEqual(3, first.Missions.Count);
            Assert.AreEqual(MissionStatus.Completed, first.Missions.Single(m => m.Type == DailyMissionType.Login).Status);
            CollectionAssert.AreEqual(first.Missions.Select(m => m.Type).ToArray(), second.Missions.Select(m => m.Type).ToArray());
            Assert.AreEqual(3, await Db.CreateDbContext().DailyMission.CountAsync(), "reler não duplica");
            Assert.AreEqual("2026-10-06", first.Day);
            Assert.AreEqual(new DateTime(2026, 10, 7, 3, 0, 0, DateTimeKind.Utc), first.ResetsAtUtc);
            Assert.IsFalse(first.Chest.CanClaim);
            Assert.AreEqual(0, first.Streak.Days);
            Assert.AreEqual(100, first.Chest.Reward.Ores.Single().Quantity);
        }

        [TestMethod]
        public async Task State_NewDay_CreatesNewMissions_AndKeepsTheOldOnes()
        {
            await UnlockedPlayerAsync();
            await Players.GetStateAsync(7);

            Time.Advance(TimeSpan.FromHours(24));
            var next = (await Players.GetStateAsync(7)).Daily!;

            Assert.AreEqual("2026-10-07", next.Day);
            Assert.AreEqual(MissionStatus.Completed, next.Missions.Single(m => m.Type == DailyMissionType.Login).Status);
            Assert.AreEqual(6, await Db.CreateDbContext().DailyMission.CountAsync());
        }

        // ---------------------------------------------------------------- avanço

        [TestMethod]
        public async Task Mine_AdvancesEnergyQuartzAndGems()
        {
            await UnlockedPlayerAsync();
            await SetProductionAsync(OreType.Sapphire);
            await SetTodayAsync(DailyMissionType.Energy, DailyMissionType.Quartz, DailyMissionType.Gems);
            Random.Value = 0.0; // primeiro item da tabela: Safira

            var res = await Mines.MineAsync(7, 1, 5);

            Assert.AreEqual(5, (await RowAsync(DailyMissionType.Energy)).Progress);
            Assert.AreEqual(0, (await RowAsync(DailyMissionType.Quartz)).Progress, "só safira caiu");
            Assert.AreEqual(5, (await RowAsync(DailyMissionType.Gems)).Progress);
            Assert.AreEqual(2, res.DailyMissions.Count);
        }

        [TestMethod]
        public async Task Mine_QuartzProduction_CountsQuartz_AndCompletesEnergy()
        {
            await UnlockedPlayerAsync();
            await SetTodayAsync(DailyMissionType.Energy, DailyMissionType.Quartz);
            await using (var ctx = Db.CreateDbContext())
            {
                (await ctx.Player.SingleAsync()).Energy = 100;
                await ctx.SaveChangesAsync();
            }
            Random.Value = 0.0;

            var res = await Mines.MineAsync(7, 1, 50);

            Assert.AreEqual(MissionStatus.Completed, res.DailyMissions.Single(d => d.Type == DailyMissionType.Energy).Status);
            Assert.AreEqual(50, (await RowAsync(DailyMissionType.Quartz)).Progress);
        }

        [TestMethod]
        public async Task CollectCart_AdvancesCartQuartzAndGems()
        {
            await UnlockedPlayerAsync();
            await SetTodayAsync(DailyMissionType.Cart, DailyMissionType.Quartz);
            Random.Value = 0.0;
            Time.Advance(TimeSpan.FromHours(1)); // 19 minérios (0,33/min)

            var res = await Mines.CollectCartAsync(7, 1);

            Assert.AreEqual(MissionStatus.Completed, res.DailyMissions.Single(d => d.Type == DailyMissionType.Cart).Status);
            Assert.AreEqual(res.Total, (await RowAsync(DailyMissionType.Quartz)).Progress);
        }

        [TestMethod]
        public async Task Craft_AdvancesPolishAndFacet_ByTimes()
        {
            await UnlockedPlayerAsync();
            await SetSkillAsync(SkillType.Workshop, 2);
            await GiveOresAsync(OreType.Ruby, 30);
            await GiveOresAsync(OreType.Quartz, 2000);
            await SetTodayAsync(DailyMissionType.Polish, DailyMissionType.Facet);

            var polish = await Workshop.CraftAsync(7, "Ruby", "Polished", 3);
            Assert.AreEqual(MissionStatus.Completed, polish.DailyMissions.Single().Status);

            var facet = await Workshop.CraftAsync(7, "Ruby", "Faceted", 1);
            Assert.AreEqual(MissionStatus.Completed, facet.DailyMissions.Single().Status);
        }

        [TestMethod]
        public async Task Actions_WithoutTodaysMissions_ChangeNothingAndDoNotFail()
        {
            await UnlockedPlayerAsync();
            await SetTodayAsync(); // sem nenhuma

            var res = await Mines.MineAsync(7, 1, 1);

            Assert.AreEqual(0, res.DailyMissions.Count);
        }

        [TestMethod]
        public async Task YesterdaysMissions_DoNotAdvanceToday()
        {
            await UnlockedPlayerAsync();
            await SetTodayAsync(DailyMissionType.Energy);
            Time.Advance(TimeSpan.FromHours(24));

            await Mines.MineAsync(7, 1, 3);

            await using var ctx = Db.CreateDbContext();
            Assert.AreEqual(0, (await ctx.DailyMission.SingleAsync(d => d.DailyType == DailyMissionType.Energy)).Progress);
        }

        // ---------------------------------------------------------------- resgate

        [TestMethod]
        public async Task Claim_PaysXpAndQuartz_AndMarksClaimed()
        {
            await UnlockedPlayerAsync();
            await SetTodayAsync(DailyMissionType.Login);
            await using (var ctx = Db.CreateDbContext())
            {
                var row = await ctx.DailyMission.SingleAsync();
                row.Status = MissionStatus.Completed;
                await ctx.SaveChangesAsync();
            }
            int quartzBefore = await InventoryOf(OreType.Quartz);
            int xpBefore = (await PlayerRow()).Experience;

            var res = await Daily.ClaimAsync(7, "Login");

            Assert.AreEqual(5, res.Rewards.Xp);
            Assert.AreEqual(quartzBefore + 20, await InventoryOf(OreType.Quartz));
            Assert.AreEqual(xpBefore + 5, (await PlayerRow()).Experience);
            Assert.AreEqual(MissionStatus.Claimed, res.Daily.Missions.Single().Status);
        }

        [TestMethod]
        public async Task Claim_Errors()
        {
            await UnlockedPlayerAsync();
            await SetTodayAsync(DailyMissionType.Energy, DailyMissionType.Login);
            await using (var ctx = Db.CreateDbContext())
            {
                (await ctx.DailyMission.SingleAsync(d => d.DailyType == DailyMissionType.Login)).Status = MissionStatus.Completed;
                await ctx.SaveChangesAsync();
            }

            Assert.AreEqual(ShardsErrorCode.DailyNotCompleted, (await Assert.ThrowsExactlyAsync<ShardsException>(() => Daily.ClaimAsync(7, "Energy"))).Code);
            Assert.AreEqual(ShardsErrorCode.DailyNotFound, (await Assert.ThrowsExactlyAsync<ShardsException>(() => Daily.ClaimAsync(7, "Cart"))).Code, "não é uma das de hoje");
            Assert.AreEqual(ShardsErrorCode.DailyNotFound, (await Assert.ThrowsExactlyAsync<ShardsException>(() => Daily.ClaimAsync(7, "Nope"))).Code);
            Assert.AreEqual(ShardsErrorCode.DailyNotFound, (await Assert.ThrowsExactlyAsync<ShardsException>(() => Daily.ClaimAsync(7, null))).Code);

            await Daily.ClaimAsync(7, "Login");
            Assert.AreEqual(ShardsErrorCode.DailyAlreadyClaimed, (await Assert.ThrowsExactlyAsync<ShardsException>(() => Daily.ClaimAsync(7, "Login"))).Code);
        }

        [TestMethod]
        public async Task Claim_OfYesterdaysMission_IsNotFound()
        {
            await UnlockedPlayerAsync();
            await SetTodayAsync(DailyMissionType.Login);
            await using (var ctx = Db.CreateDbContext())
            {
                (await ctx.DailyMission.SingleAsync()).Status = MissionStatus.Completed;
                await ctx.SaveChangesAsync();
            }
            Time.Advance(TimeSpan.FromHours(24)); // virou o dia sem resgatar

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Daily.ClaimAsync(7, "Login"));

            Assert.AreEqual(ShardsErrorCode.DailyNotFound, ex.Code);
        }

        // ---------------------------------------------------------------- baú e streak

        [TestMethod]
        public async Task Chest_NeedsEveryMissionClaimed()
        {
            await UnlockedPlayerAsync();
            var state = (await Players.GetStateAsync(7)).Daily!;

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Daily.ClaimChestAsync(7));
            Assert.AreEqual(ShardsErrorCode.ChestNotReady, ex.Code);
            Assert.IsFalse(state.Chest.CanClaim);
        }

        [TestMethod]
        public async Task Chest_WithoutTodaysMissions_IsNotReady()
        {
            await UnlockedPlayerAsync();
            await SetTodayAsync();

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Daily.ClaimChestAsync(7));

            Assert.AreEqual(ShardsErrorCode.ChestNotReady, ex.Code);
        }

        [TestMethod]
        public async Task Chest_PaysOnce_AndStartsTheStreak()
        {
            await UnlockedPlayerAsync();
            await ClaimAllTodayAsync();
            Assert.IsTrue((await Players.GetStateAsync(7)).Daily!.Chest.CanClaim);
            int quartzBefore = await InventoryOf(OreType.Quartz);

            var res = await Daily.ClaimChestAsync(7);

            Assert.AreEqual(10, res.Rewards.Xp);
            Assert.AreEqual(quartzBefore + 100, await InventoryOf(OreType.Quartz));
            Assert.AreEqual(1, res.Daily.Streak.Days);
            Assert.IsTrue(res.Daily.Chest.Claimed);
            Assert.IsFalse(res.Daily.Chest.CanClaim);
            Assert.AreEqual(120, res.Daily.Chest.Reward.Ores.Single().Quantity, "o próximo baú (dia 2 da sequência)");

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Daily.ClaimChestAsync(7));
            Assert.AreEqual(ShardsErrorCode.ChestAlreadyClaimed, ex.Code);
            Assert.AreEqual(quartzBefore + 100, await InventoryOf(OreType.Quartz), "o segundo resgate não paga");
        }

        [TestMethod]
        public async Task Streak_GrowsOnConsecutiveDays_AndBreaksWhenADayIsMissed()
        {
            await UnlockedPlayerAsync();

            await ClaimAllTodayAsync();
            Assert.AreEqual(1, (await Daily.ClaimChestAsync(7)).Daily.Streak.Days);

            Time.Advance(TimeSpan.FromHours(24));
            await ClaimAllTodayAsync();
            int before = await InventoryOf(OreType.Quartz);
            var second = await Daily.ClaimChestAsync(7);
            Assert.AreEqual(2, second.Daily.Streak.Days);
            Assert.AreEqual(before + 120, await InventoryOf(OreType.Quartz), "dia 2 da sequência: 100 + 20");

            Time.Advance(TimeSpan.FromHours(48)); // faltou um dia
            var broken = (await Players.GetStateAsync(7)).Daily!;
            Assert.AreEqual(0, broken.Streak.Days, "já aparece quebrada");
            Assert.AreEqual(100, broken.Chest.Reward.Ores.Single().Quantity, "recomeça no valor base");

            await ClaimAllTodayAsync();
            Assert.AreEqual(1, (await Daily.ClaimChestAsync(7)).Daily.Streak.Days);
        }

        [TestMethod]
        public async Task Streak_SeventhDay_PaysTheCycleBonus()
        {
            await UnlockedPlayerAsync();
            for (int day = 1; day <= 6; day++)
            {
                await ClaimAllTodayAsync();
                await Daily.ClaimChestAsync(7);
                Time.Advance(TimeSpan.FromHours(24));
            }

            await ClaimAllTodayAsync();
            int before = await InventoryOf(OreType.Quartz);
            var seventh = await Daily.ClaimChestAsync(7);

            Assert.AreEqual(7, seventh.Daily.Streak.Days);
            Assert.AreEqual(before + 440, await InventoryOf(OreType.Quartz));
            Assert.AreEqual(100, seventh.Daily.Chest.Reward.Ores.Single().Quantity, "depois do ciclo recomeça");
        }

        [TestMethod]
        public async Task Chest_ClaimedLateInTheDay_StillCountsForThatDay()
        {
            await UnlockedPlayerAsync();
            await ClaimAllTodayAsync();

            Time.Advance(TimeSpan.FromHours(10)); // 22:00 UTC do mesmo dia do jogo
            var res = await Daily.ClaimChestAsync(7);

            Assert.AreEqual(1, res.Daily.Streak.Days);
        }
    }
}
