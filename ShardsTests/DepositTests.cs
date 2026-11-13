using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shards.Config;
using Shards.Errors;
using Shards.Model.DTO;
using Shards.Service;

namespace ShardsTests
{
    [TestClass]
    public class DepositRulesTests : GameRulesTestBase
    {
        // 2026-10-05 é segunda-feira; o reset das diárias (e da semana) é às 03:00 UTC
        private static readonly DateOnly Monday = new(2026, 10, 5);

        [TestMethod]
        public void GameWeek_TurnsOnMondayAtTheDailyResetHour()
        {
            Time.Now = new DateTimeOffset(2026, 10, 11, 2, 59, 0, TimeSpan.Zero);
            Assert.AreEqual(Monday, Rules.GameWeek, "domingo 02:59 UTC");

            Time.Now = new DateTimeOffset(2026, 10, 11, 3, 0, 0, TimeSpan.Zero);
            Assert.AreEqual(Monday, Rules.GameWeek, "domingo 03:00 UTC: o dia virou, a semana não");

            Time.Now = new DateTimeOffset(2026, 10, 12, 2, 59, 0, TimeSpan.Zero);
            Assert.AreEqual(Monday, Rules.GameWeek, "segunda 02:59 UTC ainda é o domingo do jogo");

            Time.Now = new DateTimeOffset(2026, 10, 12, 3, 0, 0, TimeSpan.Zero);
            Assert.AreEqual(new DateOnly(2026, 10, 12), Rules.GameWeek, "segunda 03:00 UTC (meia-noite em Brasília)");
        }

        [TestMethod]
        public void NextWeekResetUtc_IsTheNextMondayAtResetHour()
        {
            Assert.AreEqual(new DateTime(2026, 10, 12, 3, 0, 0, DateTimeKind.Utc), Rules.NextWeekResetUtc);

            Time.Now = new DateTimeOffset(2026, 10, 12, 3, 0, 0, TimeSpan.Zero);
            Assert.AreEqual(new DateTime(2026, 10, 19, 3, 0, 0, DateTimeKind.Utc), Rules.NextWeekResetUtc);
        }

        [TestMethod]
        public void RerollCost_FirstIsFree_ThenQuartzDoubling()
        {
            Assert.IsNull(Rules.RerollCost(0));

            Assert.AreEqual(500, Rules.RerollCost(1)!.Quantity);
            Assert.AreEqual(OreType.Quartz, Rules.RerollCost(1)!.Ore);
            Assert.AreEqual(1000, Rules.RerollCost(2)!.Quantity);
            Assert.AreEqual(2000, Rules.RerollCost(3)!.Quantity);
            Assert.AreEqual(4000, Rules.RerollCost(4)!.Quantity);
        }

        [TestMethod]
        public void RerollCost_DoesNotOverflow()
        {
            Assert.AreEqual(int.MaxValue, Rules.RerollCost(200)!.Quantity);
        }

        [TestMethod]
        public void EffectiveStatus_SealedFromPastWeekIsExpired_OpenPersists()
        {
            DepositDTO Make(DateOnly week, DepositStatus status) => new()
            {
                PlayerId = 1, Slot = 1, WeekStart = week, Element = Element.Fire, DefenseMax = 10, DefenseCurrent = 10,
                StockMax = 10, StockRemaining = 10, Status = status, CreatedAt = Start,
            };

            Assert.AreEqual(DepositStatus.Sealed, Rules.EffectiveStatus(Make(Monday, DepositStatus.Sealed)));
            Assert.AreEqual(DepositStatus.Expired, Rules.EffectiveStatus(Make(Monday.AddDays(-7), DepositStatus.Sealed)));
            Assert.AreEqual(DepositStatus.Open, Rules.EffectiveStatus(Make(Monday.AddDays(-7), DepositStatus.Open)));
            Assert.AreEqual(DepositStatus.Depleted, Rules.EffectiveStatus(Make(Monday.AddDays(-7), DepositStatus.Depleted)));
        }

        [TestMethod]
        public void AppSettings_Tiers_MatchTheDecisions()
        {
            Assert.AreEqual(3, Balance.Deposits.Tiers.Count);
            CollectionAssert.AreEqual(new[] { 200, 400, 800 }, Balance.Deposits.Tiers.OrderBy(t => t.Slot).Select(t => t.Defense).ToArray());
            Assert.AreEqual(2.5, Balance.Deposits.StockPerDefense, 1e-9);
            Assert.AreEqual(1, Balance.Deposits.UnlockWorkshopLevel);
            Assert.IsTrue(Balance.Deposits.Tiers.All(t => t.Drops.Single().Ore == OreType.Amethyst), "recompensa padrão: 100% ametista");

            Assert.AreEqual(72, Balance.Explore.MaxEnergy, "um dia cheio de regeneração (1 ponto a cada 20 min)");
            Assert.AreEqual(1200, Balance.Explore.SecondsPerPoint);
        }

        [TestMethod]
        public void Roll_IsDeterministic_AndWithinTheTierRange()
        {
            foreach (DepositTierOptions tier in Balance.Deposits.Tiers)
            {
                double min = tier.Defense * (1 - tier.DefenseVariance);
                double max = tier.Defense * (1 + tier.DefenseVariance);

                for (int player = 1; player <= 50; player++)
                {
                    DepositRoll a = DepositTracker.Roll(Rules, tier, player, Monday, 0, null);
                    DepositRoll b = DepositTracker.Roll(Rules, tier, player, Monday, 0, null);

                    Assert.AreEqual(a, b, "mesma semente, mesma oferta");
                    Assert.IsTrue(a.Defense >= Math.Floor(min) && a.Defense <= Math.Ceiling(max), $"defesa {a.Defense} fora de {min}..{max}");
                    Assert.AreEqual((int)Math.Round(a.Defense * 2.5, MidpointRounding.AwayFromZero), a.Stock, "estoque proporcional à defesa");
                    Assert.IsTrue(Rules.GemOres.Select(Rules.ElementOf).Contains(a.Element), "o elemento sai das gemas do catálogo");
                }
            }
        }

        [TestMethod]
        public void Roll_WithExclude_NeverRepeatsTheCurrentElement()
        {
            DepositTierOptions tier = Balance.Deposits.Tiers[0];

            for (int reroll = 1; reroll <= 100; reroll++)
            {
                Element current = (Element)(1 + reroll % 3);
                Assert.AreNotEqual(current, DepositTracker.Roll(Rules, tier, 7, Monday, reroll, current).Element);
            }
        }

        [TestMethod]
        public void Roll_AllThreeElementsAppearAcrossPlayers()
        {
            var seen = Enumerable.Range(1, 200)
                .Select(p => DepositTracker.Roll(Rules, Balance.Deposits.Tiers[0], p, Monday, 0, null).Element)
                .ToHashSet();

            CollectionAssert.AreEquivalent(new[] { Element.Fire, Element.Water, Element.Wind }, seen.ToArray());
        }
    }

    [TestClass]
    public class DepositValidatorTests : GameRulesTestBase
    {
        private IEnumerable<string> Failures => new GameBalanceOptionsValidator().Validate(null, Balance).Failures ?? [];

        [TestMethod]
        public void RealAppSettings_AreValid()
        {
            Assert.IsTrue(new GameBalanceOptionsValidator().Validate(null, Balance).Succeeded);
        }

        [TestMethod]
        public void Explore_MustBePositive()
        {
            Balance.Explore.MaxEnergy = 0;
            Balance.Explore.SecondsPerPoint = 0;

            Assert.IsTrue(Failures.Any(f => f.Contains("Explore.MaxEnergy")));
            Assert.IsTrue(Failures.Any(f => f.Contains("Explore.SecondsPerPoint")));
        }

        [TestMethod]
        public void Tiers_CannotBeEmpty_OrSkipSlots()
        {
            Balance.Deposits.Tiers[1].Slot = 5;

            Assert.IsTrue(Failures.Any(f => f.Contains("Deposits.Tiers") && f.Contains("sem repetir nem pular")));

            Balance.Deposits.Tiers.Clear();
            Assert.IsTrue(Failures.Any(f => f.Contains("Deposits.Tiers não pode ser vazio")));
        }

        [TestMethod]
        public void Tier_RejectsBadDefenseVarianceAndDrops()
        {
            Balance.Deposits.Tiers[0].Defense = 0;
            Balance.Deposits.Tiers[0].DefenseVariance = 1;
            Balance.Deposits.Tiers[1].Drops.Clear();
            Balance.Deposits.Tiers[2].Drops[0].Chance = 0;

            Assert.IsTrue(Failures.Any(f => f.Contains("Tiers[1]") && f.Contains("Defense deve ser maior")));
            Assert.IsTrue(Failures.Any(f => f.Contains("Tiers[1]") && f.Contains("DefenseVariance")));
            Assert.IsTrue(Failures.Any(f => f.Contains("Tiers[2]") && f.Contains("Drops não pode ser vazio")));
            Assert.IsTrue(Failures.Any(f => f.Contains("Tiers[3]") && f.Contains("Chance")));
        }

        [TestMethod]
        public void Drops_CannotPayQuartz()
        {
            Balance.Deposits.Tiers[0].Drops.Add(new DropOptions { Ore = OreType.Quartz, Chance = 1 });

            Assert.IsTrue(Failures.Any(f => f.Contains("laço de quartzo")));
        }

        [TestMethod]
        public void Reroll_RejectsBadValues()
        {
            Balance.Deposits.Reroll.FreePerWeek = -1;
            Balance.Deposits.Reroll.Cost.Quantity = 0;
            Balance.Deposits.Reroll.CostMultiplier = 0;

            Assert.IsTrue(Failures.Any(f => f.Contains("FreePerWeek")));
            Assert.IsTrue(Failures.Any(f => f.Contains("Reroll.Cost.Quantity")));
            Assert.IsTrue(Failures.Any(f => f.Contains("CostMultiplier")));
        }

        [TestMethod]
        public void Reroll_MustBeChargedInTheCurrency()
        {
            Balance.Deposits.Reroll.Cost.Ore = OreType.Ruby;

            Assert.IsTrue(Failures.Any(f => f.Contains("Deposits.Reroll.Cost deve ser cobrado na moeda")));
        }

        [TestMethod]
        public void Unlock_MustBeWithinTheWorkshopLevels()
        {
            Balance.Deposits.UnlockWorkshopLevel = 0;
            Assert.IsTrue(Failures.Any(f => f.Contains("UnlockWorkshopLevel")));

            Balance.Deposits.UnlockWorkshopLevel = 3;
            Assert.IsTrue(Failures.Any(f => f.Contains("UnlockWorkshopLevel")));
        }

        [TestMethod]
        public void StockAndMaxOpen_MustBePositive()
        {
            Balance.Deposits.StockPerDefense = 0;
            Balance.Deposits.MaxOpen = 0;

            Assert.IsTrue(Failures.Any(f => f.Contains("StockPerDefense")));
            Assert.IsTrue(Failures.Any(f => f.Contains("MaxOpen")));
        }
    }

    [TestClass]
    public class DepositStateTests : ShardsServiceTestBase
    {
        // os records trazem listas (comparação por referência): compara pelo JSON
        private static string Json(object value) => JsonSerializer.Serialize(value);

        private DepositService Deposits => new(Db, Rules, new ShardsMapper(Rules));

        private async Task SetWorkshopAsync(int level, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            ctx.PlayerSkill.Add(new PlayerSkillDTO { PlayerId = player.Id, SkillType = SkillType.Workshop, Level = level });
            await ctx.SaveChangesAsync();
        }

        private async Task<int> RerollCountAsync(int depositId)
        {
            await using var ctx = Db.CreateDbContext();
            return (await ctx.Deposit.AsNoTracking().SingleAsync(d => d.Id == depositId)).RerollCount;
        }

        private async Task SetStatusAsync(int depositId, DepositStatus status)
        {
            await using var ctx = Db.CreateDbContext();
            var deposit = await ctx.Deposit.SingleAsync(d => d.Id == depositId);
            deposit.Status = status;
            await ctx.SaveChangesAsync();
        }

        private async Task<int> QuartzAsync(int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            return await ctx.InventoryItem.Where(i => i.PlayerId == player.Id && i.OreType == OreType.Quartz && i.Stage == ItemStage.Raw)
                .Select(i => i.Quantity).FirstOrDefaultAsync();
        }

        [TestMethod]
        public async Task State_WithoutWorkshop_HasNoDeposits_AndWritesNothing()
        {
            await SeedPlayerAsync();

            var state = await Players.GetStateAsync(7);

            Assert.IsNull(state.Deposits);
            await using var ctx = Db.CreateDbContext();
            Assert.AreEqual(0, await ctx.Deposit.CountAsync());
        }

        [TestMethod]
        public async Task State_WithWorkshop_CreatesThreeOffersOnFirstReadOfTheWeek()
        {
            await SeedPlayerAsync();
            await SetWorkshopAsync(1);

            var state = await Players.GetStateAsync(7);

            var deposits = state.Deposits!;
            Assert.AreEqual("2026-10-05", deposits.WeekStart);
            Assert.AreEqual(new DateTime(2026, 10, 12, 3, 0, 0, DateTimeKind.Utc), deposits.ResetsAtUtc);
            Assert.AreEqual(0, deposits.OpenCount);
            Assert.AreEqual(3, deposits.MaxOpen);
            Assert.AreEqual(3, deposits.Items.Count);

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, deposits.Items.Select(i => i.Slot).ToArray());
            CollectionAssert.AreEqual(new[] { "Light", "Medium", "Hard" }, deposits.Items.Select(i => i.Tier).ToArray());

            foreach (var item in deposits.Items)
            {
                Assert.AreEqual(DepositStatus.Sealed, item.Status);
                Assert.IsTrue(item.CanRotate);
                Assert.AreEqual(deposits.ResetsAtUtc, item.ExpiresAtUtc);
                Assert.AreEqual(item.DefenseMax, item.DefenseCurrent);
                Assert.AreEqual(item.StockMax, item.StockRemaining);
                Assert.AreEqual(OreType.Amethyst, item.Drops.Single().OreType);
                Assert.AreEqual(1.0, item.Drops.Single().Chance, 1e-9);
            }

            Assert.IsTrue(deposits.Items[0].DefenseMax is >= 160 and <= 240);
            Assert.IsTrue(deposits.Items[1].DefenseMax is >= 320 and <= 480);
            Assert.IsTrue(deposits.Items[2].DefenseMax is >= 640 and <= 960);
        }

        [TestMethod]
        public async Task State_SerializesTheContractTheFrontExpects()
        {
            await SeedPlayerAsync();
            await SetWorkshopAsync(1);

            var state = await Players.GetStateAsync(7);
            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(state, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

            var player = doc.RootElement.GetProperty("player");
            Assert.AreEqual(Balance.Explore.MaxEnergy, player.GetProperty("exploreEnergy").GetInt32());
            Assert.AreEqual(Balance.Explore.MaxEnergy, player.GetProperty("exploreMaxEnergy").GetInt32());
            Assert.AreEqual(1200, player.GetProperty("exploreSecondsPerEnergy").GetInt32());
            Assert.AreEqual(JsonValueKind.Null, player.GetProperty("nextExploreEnergyAtUtc").ValueKind);

            var deposits = doc.RootElement.GetProperty("deposits");
            Assert.AreEqual("2026-10-05", deposits.GetProperty("weekStart").GetString());
            Assert.AreEqual(3, deposits.GetProperty("maxOpen").GetInt32());
            Assert.AreEqual(JsonValueKind.Null, deposits.GetProperty("rerolls").GetProperty("nextCost").ValueKind);

            var item = deposits.GetProperty("items")[0];
            Assert.AreEqual("Light", item.GetProperty("tier").GetString());
            Assert.AreEqual("Sealed", item.GetProperty("status").GetString());
            Assert.IsTrue(new[] { "Fire", "Water", "Wind" }.Contains(item.GetProperty("element").GetString()));
            Assert.AreEqual("Amethyst", item.GetProperty("drops")[0].GetProperty("oreType").GetString());
            Assert.IsTrue(item.GetProperty("canRotate").GetBoolean());
        }

        [TestMethod]
        public async Task State_WithoutWorkshop_SerializesDepositsAsNull()
        {
            await SeedPlayerAsync();

            var state = await Players.GetStateAsync(7);
            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(state, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

            Assert.AreEqual(JsonValueKind.Null, doc.RootElement.GetProperty("deposits").ValueKind);
        }

        [TestMethod]
        public async Task State_RepeatedReads_ReturnTheSameOffers_AndDoNotDuplicateRows()
        {
            await SeedPlayerAsync();
            await SetWorkshopAsync(1);

            var first = (await Players.GetStateAsync(7)).Deposits!;
            var second = (await Players.GetStateAsync(7)).Deposits!;

            Assert.AreEqual(Json(first), Json(second));
            await using var ctx = Db.CreateDbContext();
            Assert.AreEqual(3, await ctx.Deposit.CountAsync());
        }

        [TestMethod]
        public async Task State_DifferentPlayers_CanGetDifferentOffers()
        {
            await SeedPlayerAsync(7);
            await SeedPlayerAsync(8);
            await SetWorkshopAsync(1, 7);
            await SetWorkshopAsync(1, 8);

            var a = (await Players.GetStateAsync(7)).Deposits!;
            var b = (await Players.GetStateAsync(8)).Deposits!;

            Assert.IsFalse(a.Items.Select(i => (i.Element, i.DefenseMax)).SequenceEqual(b.Items.Select(i => (i.Element, i.DefenseMax))));
        }

        [TestMethod]
        public async Task State_NewWeek_CreatesNewOffers_AndUntouchedOnesDisappear()
        {
            await SeedPlayerAsync();
            await SetWorkshopAsync(1);
            var week1 = (await Players.GetStateAsync(7)).Deposits!;

            Time.Advance(TimeSpan.FromDays(7));
            var week2 = (await Players.GetStateAsync(7)).Deposits!;

            Assert.AreEqual("2026-10-12", week2.WeekStart);
            Assert.AreEqual(3, week2.Items.Count, "as intocadas de ontem expiraram e não aparecem");
            CollectionAssert.AreNotEqual(week1.Items.Select(i => i.Id).ToArray(), week2.Items.Select(i => i.Id).ToArray());
            Assert.IsTrue(week2.Items.All(i => i.Status == DepositStatus.Sealed));

            await using var ctx = Db.CreateDbContext();
            Assert.AreEqual(6, await ctx.Deposit.CountAsync(), "o histórico fica gravado");
        }

        [TestMethod]
        public async Task State_AttackedDeposit_PersistsIntoTheNextWeek_AndCountsAsOpen()
        {
            await SeedPlayerAsync();
            await SetWorkshopAsync(1);
            var week1 = (await Players.GetStateAsync(7)).Deposits!;
            int attackedId = week1.Items[1].Id;
            await SetStatusAsync(attackedId, DepositStatus.Open);

            Time.Advance(TimeSpan.FromDays(7));
            var week2 = (await Players.GetStateAsync(7)).Deposits!;

            Assert.AreEqual(4, week2.Items.Count, "3 novas da semana mais a atacada que persiste");
            Assert.AreEqual(1, week2.OpenCount);

            var old = week2.Items.Single(i => i.Id == attackedId);
            Assert.AreEqual(DepositStatus.Open, old.Status);
            Assert.IsFalse(old.CanRotate);
            Assert.IsNull(old.ExpiresAtUtc);
            Assert.AreEqual("2026-10-12", week2.WeekStart);
            Assert.AreEqual(attackedId, week2.Items.Last().Id, "as da semana corrente vêm primeiro");
        }

        [TestMethod]
        public async Task State_RerollsBlock_StartsFreeThenShowsTheNextCost()
        {
            await SeedPlayerAsync();
            await SetWorkshopAsync(1);

            var rerolls = (await Players.GetStateAsync(7)).Deposits!.Rerolls;

            Assert.AreEqual(0, rerolls.Used);
            Assert.AreEqual(1, rerolls.FreeLeft);
            Assert.IsNull(rerolls.NextCost, "a primeira é grátis");
            Assert.IsTrue(rerolls.CanAfford);
        }

        [TestMethod]
        public async Task Rotate_FirstIsFree_ThenQuartzDoubling_AndTheElementChanges()
        {
            await SeedPlayerAsync();
            await SetWorkshopAsync(1);
            await GiveOresAsync(OreType.Quartz, 1600);
            var offers = (await Players.GetStateAsync(7)).Deposits!.Items;
            var light = offers[0];

            var first = await Deposits.RotateAsync(7, light.Id);

            Assert.IsNull(first.Spent, "troca grátis");
            Assert.AreEqual(light.Id, first.Deposit.Id, "a mesma linha é reescrita");
            Assert.AreEqual(1, first.Deposit.Slot);
            Assert.AreNotEqual(light.Element, first.Deposit.Element);
            Assert.AreEqual(1, first.Deposits.Rerolls.Used);
            Assert.AreEqual(0, first.Deposits.Rerolls.FreeLeft);
            Assert.AreEqual(500, first.Deposits.Rerolls.NextCost!.Quantity);
            Assert.AreEqual(1600, await QuartzAsync());

            var second = await Deposits.RotateAsync(7, light.Id);
            Assert.AreEqual(500, second.Spent!.Quantity);
            Assert.AreEqual(OreType.Quartz, second.Spent.OreType);
            Assert.AreNotEqual(first.Deposit.Element, second.Deposit.Element);
            Assert.AreEqual(1100, await QuartzAsync());
            Assert.AreEqual(1000, second.Deposits.Rerolls.NextCost!.Quantity);
            Assert.AreEqual(1100, second.Inventory.Single(i => i.OreType == OreType.Quartz).Quantity);

            var third = await Deposits.RotateAsync(7, light.Id);
            Assert.AreEqual(1000, third.Spent!.Quantity);
            Assert.AreEqual(100, await QuartzAsync());
            Assert.AreEqual(3, await RerollCountAsync(light.Id));
        }

        [TestMethod]
        public async Task Rotate_LimitIsForTheWholeWeek_NotPerSlot()
        {
            await SeedPlayerAsync();
            await SetWorkshopAsync(1);
            await GiveOresAsync(OreType.Quartz, 500);
            var offers = (await Players.GetStateAsync(7)).Deposits!.Items;

            var first = await Deposits.RotateAsync(7, offers[0].Id);
            var second = await Deposits.RotateAsync(7, offers[1].Id);

            Assert.IsNull(first.Spent);
            Assert.AreEqual(500, second.Spent!.Quantity, "o outro slot já paga: a grátis da semana foi usada");
            Assert.AreEqual(2, second.Deposits.Rerolls.Used);
        }

        [TestMethod]
        public async Task Rotate_Rewrites_DefenseAndStock_WithinTheTierRange()
        {
            await SeedPlayerAsync();
            await SetWorkshopAsync(1);
            var hard = (await Players.GetStateAsync(7)).Deposits!.Items[2];

            var res = await Deposits.RotateAsync(7, hard.Id);

            Assert.IsTrue(res.Deposit.DefenseMax is >= 640 and <= 960);
            Assert.AreEqual(res.Deposit.DefenseMax, res.Deposit.DefenseCurrent);
            Assert.AreEqual((int)Math.Round(res.Deposit.DefenseMax * 2.5, MidpointRounding.AwayFromZero), res.Deposit.StockMax);
            Assert.AreEqual(res.Deposit.StockMax, res.Deposit.StockRemaining);
        }

        [TestMethod]
        public async Task Rotate_WithoutQuartz_Fails_AndChangesNothing()
        {
            await SeedPlayerAsync();
            await SetWorkshopAsync(1);
            await GiveOresAsync(OreType.Quartz, 499);
            var offers = (await Players.GetStateAsync(7)).Deposits!.Items;

            await Deposits.RotateAsync(7, offers[0].Id); // a grátis

            var before = (await Players.GetStateAsync(7)).Deposits!.Items.Single(i => i.Id == offers[0].Id);
            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Deposits.RotateAsync(7, offers[0].Id));

            Assert.AreEqual(ShardsErrorCode.NotEnoughResources, ex.Code);
            Assert.AreEqual(1, await RerollCountAsync(offers[0].Id));
            Assert.AreEqual(499, await QuartzAsync());
            Assert.AreEqual(Json(before), Json((await Players.GetStateAsync(7)).Deposits!.Items.Single(i => i.Id == offers[0].Id)));
        }

        [TestMethod]
        public async Task Rotate_AlreadyAttacked_IsRefused()
        {
            await SeedPlayerAsync();
            await SetWorkshopAsync(1);
            var offers = (await Players.GetStateAsync(7)).Deposits!.Items;
            await SetStatusAsync(offers[0].Id, DepositStatus.Open);

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Deposits.RotateAsync(7, offers[0].Id));

            Assert.AreEqual(ShardsErrorCode.DepositNotRotatable, ex.Code);
            Assert.AreEqual(0, await RerollCountAsync(offers[0].Id));
        }

        [TestMethod]
        public async Task Rotate_FromPastWeekUntouched_IsExpired()
        {
            await SeedPlayerAsync();
            await SetWorkshopAsync(1);
            var offers = (await Players.GetStateAsync(7)).Deposits!.Items;

            Time.Advance(TimeSpan.FromDays(7));

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Deposits.RotateAsync(7, offers[0].Id));

            Assert.AreEqual(ShardsErrorCode.DepositExpired, ex.Code);
        }

        [TestMethod]
        public async Task Rotate_AnotherPlayersDeposit_IsNotFound()
        {
            await SeedPlayerAsync(7);
            await SeedPlayerAsync(8);
            await SetWorkshopAsync(1, 7);
            var offers = (await Players.GetStateAsync(7)).Deposits!.Items;

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Deposits.RotateAsync(8, offers[0].Id));

            Assert.AreEqual(ShardsErrorCode.DepositNotFound, ex.Code);
            Assert.AreEqual(0, await RerollCountAsync(offers[0].Id));
        }

        [TestMethod]
        public async Task Rotate_UnknownId_IsNotFound()
        {
            await SeedPlayerAsync();

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Deposits.RotateAsync(7, 9999));

            Assert.AreEqual(ShardsErrorCode.DepositNotFound, ex.Code);
        }

        [TestMethod]
        public async Task Rotate_NewWeek_ResetsTheFreeRotation()
        {
            await SeedPlayerAsync();
            await SetWorkshopAsync(1);
            var week1 = (await Players.GetStateAsync(7)).Deposits!.Items;
            await Deposits.RotateAsync(7, week1[0].Id);

            Time.Advance(TimeSpan.FromDays(7));
            var week2 = (await Players.GetStateAsync(7)).Deposits!;

            Assert.AreEqual(0, week2.Rerolls.Used);
            Assert.AreEqual(1, week2.Rerolls.FreeLeft);

            var res = await Deposits.RotateAsync(7, week2.Items[0].Id);
            Assert.IsNull(res.Spent, "a grátis voltou na semana nova");
        }
    }
}
