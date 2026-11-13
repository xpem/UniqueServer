using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shards.Config;
using Shards.Errors;
using Shards.Model.DTO;
using Shards.Model.Req;
using Shards.Rules;
using Shards.Service;

namespace ShardsTests
{
    [TestClass]
    public class DepositCombatRulesTests : GameRulesTestBase
    {
        private DepositDTO Deposit(Element element, int defense, int slot = 1) => new()
        {
            PlayerId = 1, Slot = slot, WeekStart = new DateOnly(2026, 10, 5), Element = element,
            DefenseMax = defense, DefenseCurrent = defense, StockMax = 100, StockRemaining = 100, CreatedAt = Start,
        };

        private AttackOutcome Resolve(DepositDTO deposit, GemStack[] attackers, params GemStack[] defenders) =>
            DepositCombat.Resolve(Rules, deposit, Rules.FindTier(deposit.Slot)!, attackers, defenders);

        private static GemStack Stack(OreType ore, ItemStage stage, int quantity) => new(ore, stage, quantity);

        [TestMethod]
        public void AppSettings_CombatNumbers_MatchTheDecisions()
        {
            Assert.AreEqual(3, Balance.Combat.EnergyPerCounterPoint, 1e-9);
            CollectionAssert.AreEqual(new[] { 0.05, 0.1, 0.2 }, Balance.Deposits.Tiers.OrderBy(t => t.Slot).Select(t => t.CounterRatio).ToArray());
            Assert.AreEqual(10, Balance.Deposits.MaxAttackStacks);
        }

        [TestMethod]
        public void Damage_SameElement_UsesTheCatalogAttack()
        {
            var outcome = Resolve(Deposit(Element.Fire, 100), [Stack(OreType.Ruby, ItemStage.Polished, 10)]);

            Assert.AreEqual(40, outcome.DamageDealt, "Rubi polida ataca 4 e Fogo contra Fogo vale x1");
            Assert.AreEqual(60, outcome.DefenseAfter);
            Assert.AreEqual(10, outcome.AttackersUsed.Single().Quantity);
        }

        [TestMethod]
        public void Damage_Advantage_Multiplies()
        {
            // Água vence Fogo (x1,5): Safira facetada 16 x 1,5 = 24 por gema
            var outcome = Resolve(Deposit(Element.Fire, 100), [Stack(OreType.Sapphire, ItemStage.Faceted, 2)]);

            Assert.AreEqual(48, outcome.DamageDealt);
        }

        [TestMethod]
        public void Damage_Disadvantage_Reduces()
        {
            // Vento contra Fogo é desvantagem (x0,75): Esmeralda facetada 24 x 0,75 = 18 por gema
            var outcome = Resolve(Deposit(Element.Fire, 100), [Stack(OreType.Emerald, ItemStage.Faceted, 2)]);

            Assert.AreEqual(36, outcome.DamageDealt);
        }

        [TestMethod]
        public void Damage_StopsAtZero_ConsumingOnlyTheNeededUnits()
        {
            var outcome = Resolve(Deposit(Element.Fire, 100), [Stack(OreType.Ruby, ItemStage.Faceted, 10)]);

            Assert.AreEqual(4, outcome.AttackersUsed.Single().Quantity, "ceil(100 / 32) = 4 gemas; as outras 6 não são gastas");
            Assert.AreEqual(100, outcome.DamageDealt, "o dano nunca passa da defesa que restava (128 de dano bruto)");
            Assert.AreEqual(0, outcome.DefenseAfter);
        }

        [TestMethod]
        public void Damage_AppliesStacksInTheListedOrder_AndSkipsTheRestAfterZero()
        {
            var outcome = Resolve(Deposit(Element.Fire, 70), [
                Stack(OreType.Ruby, ItemStage.Faceted, 2),
                Stack(OreType.Ruby, ItemStage.Polished, 10),
                Stack(OreType.Sapphire, ItemStage.Faceted, 5),
            ]);

            Assert.AreEqual(2, outcome.AttackersUsed.Count, "a terceira pilha nem é tocada");
            Assert.AreEqual(2, outcome.AttackersUsed[0].Quantity, "64 pontos das duas facetadas");
            Assert.AreEqual(2, outcome.AttackersUsed[1].Quantity, "restavam 6: ceil(6 / 4) = 2 polidas");
            Assert.AreEqual(70, outcome.DamageDealt);
        }

        [TestMethod]
        public void Damage_FractionalTotalRoundsUp_AtMostOnePoint()
        {
            // Esmeralda polida contra Fogo: 3 x 0,75 = 2,25 por gema
            var outcome = Resolve(Deposit(Element.Fire, 100), [Stack(OreType.Emerald, ItemStage.Polished, 1)]);

            Assert.AreEqual(3, outcome.DamageDealt, "2,25 sobe para 3");
            Assert.AreEqual(97, outcome.DefenseAfter);

            var small = Resolve(Deposit(Element.Fire, 2), [Stack(OreType.Emerald, ItemStage.Polished, 5)]);
            Assert.AreEqual(1, small.AttackersUsed.Single().Quantity, "ceil(2 / 2,25) = 1");
            Assert.AreEqual(2, small.DamageDealt, "limitado ao que restava");
        }

        [TestMethod]
        public void Damage_ImmuneElement_DealsNothing()
        {
            Balance.Combat.Effectiveness.Add(new EffectivenessOptions { Attacker = Element.Fire, Defender = Element.Fire, Multiplier = 0 });

            var outcome = Resolve(Deposit(Element.Fire, 100), [Stack(OreType.Ruby, ItemStage.Faceted, 5)]);

            Assert.AreEqual(0, outcome.DamageDealt);
            Assert.AreEqual(0, outcome.AttackersUsed.Count);
        }

        [TestMethod]
        [DataRow(1, 5.0, 15)]
        [DataRow(2, 10.0, 30)]
        [DataRow(3, 20.0, 60)]
        public void Counter_IsProportionalToTheDamage_AndPerTier(int slot, double counter, int energy)
        {
            var outcome = Resolve(Deposit(Element.Fire, 100, slot), [Stack(OreType.Ruby, ItemStage.Faceted, 10)]);

            Assert.AreEqual(counter, outcome.CounterAttack, 1e-9);
            Assert.AreEqual(counter, outcome.Unabsorbed, 1e-9, "sem defensores nada é absorvido");
            Assert.AreEqual(energy, outcome.EnergyCost, "3 de energia por ponto não absorvido");
        }

        [TestMethod]
        public void Counter_UsesTheEffectiveDamage_NotTheRawOne()
        {
            // 128 de dano bruto, mas a defesa era 100: o contra-ataque vem dos 100
            var outcome = Resolve(Deposit(Element.Fire, 100), [Stack(OreType.Ruby, ItemStage.Faceted, 10)]);

            Assert.AreEqual(5.0, outcome.CounterAttack, 1e-9);
        }

        [TestMethod]
        public void Absorb_DefenderCoversTheCounter_AndOnlyTheNeededUnitsAreSpent()
        {
            // Fogo contra Água (x0,75): Safira polida defende 4 / 0,75 = 5,33 por gema; contra-ataque 5 cabe em 1 gema
            var outcome = Resolve(Deposit(Element.Fire, 100), [Stack(OreType.Ruby, ItemStage.Faceted, 4)], Stack(OreType.Sapphire, ItemStage.Polished, 5));

            Assert.AreEqual(1, outcome.DefendersUsed.Single().Quantity);
            Assert.AreEqual(5.0, outcome.Absorbed, 1e-9);
            Assert.AreEqual(0.0, outcome.Unabsorbed, 1e-9);
            Assert.AreEqual(0, outcome.EnergyCost);
        }

        [TestMethod]
        public void Absorb_WeaknessAbsorbsLess()
        {
            // Fogo contra Vento (x1,5): Esmeralda polida defende 3 / 1,5 = 2 por gema; contra-ataque 5 pede 3 gemas
            var enough = Resolve(Deposit(Element.Fire, 100), [Stack(OreType.Ruby, ItemStage.Faceted, 4)], Stack(OreType.Emerald, ItemStage.Polished, 5));
            Assert.AreEqual(3, enough.DefendersUsed.Single().Quantity);
            Assert.AreEqual(0, enough.EnergyCost);

            // só 1 gema: absorve 2 e sobram 3 pontos, 9 de energia
            var short1 = Resolve(Deposit(Element.Fire, 100), [Stack(OreType.Ruby, ItemStage.Faceted, 4)], Stack(OreType.Emerald, ItemStage.Polished, 1));
            Assert.AreEqual(2.0, short1.Absorbed, 1e-9);
            Assert.AreEqual(3.0, short1.Unabsorbed, 1e-9);
            Assert.AreEqual(9, short1.EnergyCost);
        }

        [TestMethod]
        public void Absorb_PartialConsumption_SpendsOnlyWhatAbsorbs()
        {
            // contra-ataque 10 (dano 200 na faixa leve); Safira polida absorve 5,33: ceil(10 / 5,33) = 2 de 10
            var outcome = Resolve(Deposit(Element.Fire, 200), [Stack(OreType.Ruby, ItemStage.Faceted, 7)], Stack(OreType.Sapphire, ItemStage.Polished, 10));

            Assert.AreEqual(2, outcome.DefendersUsed.Single().Quantity);
            Assert.AreEqual(0, outcome.EnergyCost);
        }

        [TestMethod]
        public void Absorb_FollowsTheListedOrder_AcrossStacks()
        {
            // contra-ataque 10; 1 Safira facetada (32 / 0,75 = 42,7) cobre sozinha, então a segunda pilha não é tocada
            var outcome = Resolve(Deposit(Element.Fire, 200), [Stack(OreType.Ruby, ItemStage.Faceted, 7)],
                Stack(OreType.Sapphire, ItemStage.Faceted, 1), Stack(OreType.Sapphire, ItemStage.Polished, 10));

            Assert.AreEqual(1, outcome.DefendersUsed.Count);
        }

        [TestMethod]
        public void Absorb_ImmuneDefender_AbsorbsEverythingWithOneUnit()
        {
            Balance.Combat.Effectiveness.First(e => e.Attacker == Element.Fire && e.Defender == Element.Wind).Multiplier = 0;

            var outcome = Resolve(Deposit(Element.Fire, 800, 3), [Stack(OreType.Ruby, ItemStage.Faceted, 30)], Stack(OreType.Emerald, ItemStage.Polished, 5));

            Assert.AreEqual(1, outcome.DefendersUsed.Single().Quantity);
            Assert.AreEqual(0, outcome.EnergyCost);
        }

        [TestMethod]
        public void Energy_RoundsUp()
        {
            // 1 ponto de dano na faixa leve = 0,05 de contra-ataque = 0,15 de energia, que vira 1
            var outcome = Resolve(Deposit(Element.Fire, 1), [Stack(OreType.Ruby, ItemStage.Polished, 1)]);

            Assert.AreEqual(1, outcome.DamageDealt);
            Assert.AreEqual(1, outcome.EnergyCost);
        }
    }

    [TestClass]
    public class DepositCombatValidatorTests : GameRulesTestBase
    {
        private IEnumerable<string> Failures => new GameBalanceOptionsValidator().Validate(null, Balance).Failures ?? [];

        [TestMethod]
        public void RealAppSettings_AreValid()
        {
            Assert.IsTrue(new GameBalanceOptionsValidator().Validate(null, Balance).Succeeded);
        }

        [TestMethod]
        public void CounterRatio_MustExistInEveryTier()
        {
            Balance.Deposits.Tiers[0].CounterRatio = 0;
            Balance.Deposits.Tiers[1].CounterRatio = 6;

            Assert.IsTrue(Failures.Any(f => f.Contains("Tiers[1]") && f.Contains("CounterRatio")));
            Assert.IsTrue(Failures.Any(f => f.Contains("Tiers[2]") && f.Contains("CounterRatio")));
        }

        [TestMethod]
        public void EnergyPerCounterPoint_AndMaxAttackStacks_MustBePositive()
        {
            Balance.Combat.EnergyPerCounterPoint = 0;
            Balance.Deposits.MaxAttackStacks = 0;

            Assert.IsTrue(Failures.Any(f => f.Contains("EnergyPerCounterPoint")));
            Assert.IsTrue(Failures.Any(f => f.Contains("MaxAttackStacks")));
        }
    }

    [TestClass]
    public class DepositAttackTests : ShardsServiceTestBase
    {
        private DepositService Deposits => new(Db, Rules, new ShardsMapper(Rules));

        private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

        private static GemStackReq Req(string ore, string stage, int quantity) => new() { Ore = ore, Stage = stage, Quantity = quantity };

        private static AttackDepositReq Attack(GemStackReq[] attackers, GemStackReq[]? defenders = null, bool preview = false) =>
            new() { Attackers = [.. attackers], Defenders = defenders is null ? null : [.. defenders], Preview = preview };

        private async Task SetWorkshopAsync(int level, int uid = 7)
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
            var item = await ctx.InventoryItem.FirstOrDefaultAsync(i => i.PlayerId == player.Id && i.OreType == ore && i.Stage == stage);
            if (item is null) ctx.InventoryItem.Add(new InventoryItemDTO { PlayerId = player.Id, OreType = ore, Stage = stage, Quantity = quantity });
            else item.Quantity += quantity;
            await ctx.SaveChangesAsync();
        }

        private async Task<int> StagedAsync(OreType ore, ItemStage stage, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            return await ctx.InventoryItem.Where(i => i.PlayerId == player.Id && i.OreType == ore && i.Stage == stage)
                .Select(i => i.Quantity).FirstOrDefaultAsync();
        }

        /// <summary>Cria as ofertas e fixa elemento e defesa de um slot, para o cálculo ser previsível nos testes.</summary>
        private async Task<int> SeedDepositAsync(int slot = 1, Element element = Element.Fire, int defense = 100, int uid = 7)
        {
            await SeedPlayerAsync(uid);
            await SetWorkshopAsync(1, uid);
            var id = (await Players.GetStateAsync(uid)).Deposits!.Items.Single(i => i.Slot == slot).Id;
            await UpdateDepositAsync(id, d => { d.Element = element; d.DefenseMax = defense; d.DefenseCurrent = defense; });
            return id;
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

        private async Task<int> ExploreEnergyAsync(int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            return (await ctx.Player.AsNoTracking().SingleAsync(p => p.UserId == uid)).ExploreEnergy;
        }

        private static async Task<ShardsException> ThrowsAsync(Func<Task> action) => await Assert.ThrowsExactlyAsync<ShardsException>(action);

        [TestMethod]
        public async Task Attack_BreaksTheDefense_ConsumesGemsAndEnergy_AndOpensTheDeposit()
        {
            int id = await SeedDepositAsync();
            await GiveStagedAsync(OreType.Ruby, ItemStage.Faceted, 10);

            var res = await Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Faceted", 10)]));

            Assert.IsFalse(res.Preview);
            Assert.IsTrue(res.CanConfirm);
            Assert.IsNull(res.Blocker);
            Assert.AreEqual(100, res.DefenseBefore);
            Assert.AreEqual(100, res.DamageDealt);
            Assert.AreEqual(0, res.DefenseAfter);
            Assert.IsTrue(res.Broken);
            Assert.AreEqual(4, res.AttackersUsed.Single().Quantity);
            Assert.AreEqual(5.0, res.CounterAttack, 1e-9);
            Assert.AreEqual(15, res.EnergyCost, "5 pontos x 3");
            Assert.AreEqual(Balance.Explore.MaxEnergy - 15, res.ExploreEnergyAfter);

            Assert.AreEqual(6, await StagedAsync(OreType.Ruby, ItemStage.Faceted), "só as 4 necessárias foram gastas");
            Assert.AreEqual(Balance.Explore.MaxEnergy - 15, await ExploreEnergyAsync());

            var deposit = await LoadDepositAsync(id);
            Assert.AreEqual(DepositStatus.Open, deposit.Status);
            Assert.AreEqual(0, deposit.DefenseCurrent);
            Assert.AreEqual(deposit.StockMax, deposit.StockRemaining, "a extração é a fatia seguinte");

            Assert.AreEqual(DepositStatus.Open, res.Deposit.Status);
            Assert.IsFalse(res.Deposit.CanAttack, "defesa zerada");
            Assert.IsFalse(res.Deposit.CanRotate);
            Assert.AreEqual(1, res.Deposits.OpenCount);
            Assert.AreEqual(Balance.Explore.MaxEnergy - 15, res.Player.ExploreEnergy);
            Assert.AreEqual(6, res.Inventory.Items.Single(i => i.OreType == OreType.Ruby && i.Stage == ItemStage.Faceted).Quantity);
        }

        [TestMethod]
        public async Task Attack_PartialDamage_PersistsAndAccumulatesAcrossCalls()
        {
            int id = await SeedDepositAsync();
            await GiveStagedAsync(OreType.Ruby, ItemStage.Polished, 20);

            var first = await Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Polished", 10)]));
            Assert.AreEqual(60, first.DefenseAfter);
            Assert.IsFalse(first.Broken);
            Assert.AreEqual(6, first.EnergyCost, "dano 40 x 5% = 2 pontos x 3");

            var second = await Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Polished", 10)]));
            Assert.AreEqual(60, second.DefenseBefore);
            Assert.AreEqual(20, second.DefenseAfter);

            Assert.AreEqual(20, (await LoadDepositAsync(id)).DefenseCurrent);
            Assert.AreEqual(Balance.Explore.MaxEnergy - 12, await ExploreEnergyAsync(), "teto - 6 - 6");
            Assert.AreEqual(0, await StagedAsync(OreType.Ruby, ItemStage.Polished));
        }

        [TestMethod]
        public async Task Attack_WithDefenders_AbsorbsTheCounter_AndDoesNotSpendEnergy()
        {
            int id = await SeedDepositAsync();
            await GiveStagedAsync(OreType.Ruby, ItemStage.Faceted, 4);
            await GiveStagedAsync(OreType.Sapphire, ItemStage.Polished, 5);

            var res = await Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Faceted", 4)], [Req("Sapphire", "Polished", 5)]));

            Assert.AreEqual(0, res.EnergyCost);
            Assert.AreEqual(5.0, res.Absorbed, 1e-9);
            Assert.AreEqual(1, res.DefendersUsed.Single().Quantity);
            Assert.AreEqual(4, await StagedAsync(OreType.Sapphire, ItemStage.Polished), "só a gema que absorveu foi gasta");
            Assert.AreEqual(Balance.Explore.MaxEnergy, await ExploreEnergyAsync());
        }

        [TestMethod]
        public async Task Attack_ExploreEnergy_KeepsThePartialRegenerationRemainder()
        {
            int id = await SeedDepositAsync();
            await GiveStagedAsync(OreType.Ruby, ItemStage.Polished, 10);

            await using (var ctx = Db.CreateDbContext())
            {
                var player = await ctx.Player.SingleAsync(p => p.UserId == 7);
                player.ExploreEnergy = 10;
                player.LastExploreEnergyUpdateUtc = Start.AddSeconds(-1800); // 1,5 ponto atrás
                await ctx.SaveChangesAsync();
            }

            var res = await Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Polished", 10)]));

            Assert.AreEqual(6, res.EnergyCost);
            Assert.AreEqual(10 + 1 - 6, await ExploreEnergyAsync(), "1 ponto regenerou antes de debitar");
            await using var verify = Db.CreateDbContext();
            Assert.AreEqual(Start.AddSeconds(-1800 + 1200), (await verify.Player.SingleAsync(p => p.UserId == 7)).LastExploreEnergyUpdateUtc, "o resto de 600 s foi preservado");
        }

        [TestMethod]
        public async Task Preview_WritesNothing_AndMatchesTheRealAttack()
        {
            int id = await SeedDepositAsync();
            await GiveStagedAsync(OreType.Ruby, ItemStage.Faceted, 4);
            await GiveStagedAsync(OreType.Emerald, ItemStage.Polished, 5);
            var request = Attack([Req("Ruby", "Faceted", 4)], [Req("Emerald", "Polished", 5)]);

            var preview = await Deposits.AttackAsync(7, id, request with { Preview = true });

            Assert.IsTrue(preview.Preview);
            Assert.IsTrue(preview.CanConfirm);
            Assert.AreEqual(4, await StagedAsync(OreType.Ruby, ItemStage.Faceted));
            Assert.AreEqual(5, await StagedAsync(OreType.Emerald, ItemStage.Polished));
            Assert.AreEqual(Balance.Explore.MaxEnergy, await ExploreEnergyAsync());
            var untouched = await LoadDepositAsync(id);
            Assert.AreEqual(DepositStatus.Sealed, untouched.Status);
            Assert.AreEqual(100, untouched.DefenseCurrent);
            Assert.AreEqual(100, preview.Deposit.DefenseCurrent, "na prévia o depósito vem como está agora");

            var real = await Deposits.AttackAsync(7, id, request);

            Assert.AreEqual(preview.DamageDealt, real.DamageDealt);
            Assert.AreEqual(preview.DefenseAfter, real.DefenseAfter);
            Assert.AreEqual(preview.EnergyCost, real.EnergyCost);
            Assert.AreEqual(preview.ExploreEnergyAfter, real.ExploreEnergyAfter);
            Assert.AreEqual(JsonSerializer.Serialize(preview.AttackersUsed), JsonSerializer.Serialize(real.AttackersUsed));
            Assert.AreEqual(JsonSerializer.Serialize(preview.DefendersUsed), JsonSerializer.Serialize(real.DefendersUsed));
            Assert.AreEqual(preview.CounterAttack, real.CounterAttack);
            Assert.AreEqual(preview.Absorbed, real.Absorbed);
        }

        [TestMethod]
        public async Task Preview_WhenSomethingBlocks_ReportsTheBlockerInsteadOfThrowing()
        {
            int id = await SeedDepositAsync();
            await GiveStagedAsync(OreType.Ruby, ItemStage.Faceted, 4);
            await using (var ctx = Db.CreateDbContext())
            {
                var player = await ctx.Player.SingleAsync(p => p.UserId == 7);
                player.ExploreEnergy = 5; // o ataque custaria 15
                await ctx.SaveChangesAsync();
            }

            var energy = await Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Faceted", 4)], preview: true));
            Assert.IsFalse(energy.CanConfirm);
            Assert.AreEqual("NotEnoughExploreEnergy", energy.Blocker);
            Assert.AreEqual(15, energy.EnergyCost);
            Assert.AreEqual(0, energy.ExploreEnergyAfter, "nunca negativo");

            var gems = await Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Faceted", 400)], preview: true));
            Assert.IsFalse(gems.CanConfirm);
            Assert.AreEqual("NotEnoughResources", gems.Blocker);
        }

        [TestMethod]
        public async Task Attack_WithoutEnoughExploreEnergy_IsRefused_AndChangesNothing()
        {
            int id = await SeedDepositAsync();
            await GiveStagedAsync(OreType.Ruby, ItemStage.Faceted, 4);
            await using (var ctx = Db.CreateDbContext())
            {
                var player = await ctx.Player.SingleAsync(p => p.UserId == 7);
                player.ExploreEnergy = 14;
                await ctx.SaveChangesAsync();
            }

            var ex = await ThrowsAsync(() => Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Faceted", 4)])));

            Assert.AreEqual(ShardsErrorCode.NotEnoughExploreEnergy, ex.Code);
            Assert.AreEqual(4, await StagedAsync(OreType.Ruby, ItemStage.Faceted));
            Assert.AreEqual(14, await ExploreEnergyAsync());
            var deposit = await LoadDepositAsync(id);
            Assert.AreEqual(DepositStatus.Sealed, deposit.Status);
            Assert.AreEqual(100, deposit.DefenseCurrent);
        }

        [TestMethod]
        public async Task Attack_ListedGemsMustExist_EvenIfNotAllAreNeeded()
        {
            int id = await SeedDepositAsync();
            await GiveStagedAsync(OreType.Ruby, ItemStage.Faceted, 3);

            var ex = await ThrowsAsync(() => Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Faceted", 10)])));

            Assert.AreEqual(ShardsErrorCode.NotEnoughResources, ex.Code);
            Assert.AreEqual(3, await StagedAsync(OreType.Ruby, ItemStage.Faceted));
            Assert.AreEqual(DepositStatus.Sealed, (await LoadDepositAsync(id)).Status);
        }

        [TestMethod]
        public async Task Attack_TheSameGemInBothLists_IsSummedAgainstTheInventory()
        {
            int id = await SeedDepositAsync();
            await GiveStagedAsync(OreType.Ruby, ItemStage.Faceted, 6);

            var ex = await ThrowsAsync(() => Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Faceted", 5)], [Req("Ruby", "Faceted", 5)])));

            Assert.AreEqual(ShardsErrorCode.NotEnoughResources, ex.Code, "5 + 5 passa dos 6 que existem");
        }

        [TestMethod]
        public async Task Attack_RepeatedStacks_AreMerged()
        {
            int id = await SeedDepositAsync(defense: 1000);
            await GiveStagedAsync(OreType.Ruby, ItemStage.Faceted, 4);

            var res = await Deposits.AttackAsync(7, id, Attack([Req("ruby", "faceted", 2), Req("Ruby", "Faceted", 2)], preview: true));

            Assert.AreEqual(1, res.AttackersUsed.Count);
            Assert.AreEqual(4, res.AttackersUsed.Single().Quantity);
            Assert.AreEqual(128, res.DamageDealt);
        }

        [TestMethod]
        public async Task Attack_OpeningAFourthDeposit_IsRefused_ButAttackingAnOpenOneIsNot()
        {
            Balance.Deposits.MaxOpen = 1;
            int light = await SeedDepositAsync(slot: 1);
            int medium = (await Players.GetStateAsync(7)).Deposits!.Items.Single(i => i.Slot == 2).Id;
            await UpdateDepositAsync(medium, d => { d.Element = Element.Fire; d.DefenseMax = 400; d.DefenseCurrent = 400; });
            await GiveStagedAsync(OreType.Ruby, ItemStage.Polished, 100);

            await Deposits.AttackAsync(7, light, Attack([Req("Ruby", "Polished", 10)]));

            var state = (await Players.GetStateAsync(7)).Deposits!;
            Assert.IsTrue(state.Items.Single(i => i.Id == light).CanAttack, "um já aberto continua atacável");
            Assert.IsFalse(state.Items.Single(i => i.Id == medium).CanAttack, "abrir mais um passaria do limite");

            var ex = await ThrowsAsync(() => Deposits.AttackAsync(7, medium, Attack([Req("Ruby", "Polished", 10)])));
            Assert.AreEqual(ShardsErrorCode.TooManyOpenDeposits, ex.Code);
            Assert.AreEqual(DepositStatus.Sealed, (await LoadDepositAsync(medium)).Status);

            var preview = await Deposits.AttackAsync(7, medium, Attack([Req("Ruby", "Polished", 10)], preview: true));
            Assert.IsFalse(preview.CanConfirm);
            Assert.AreEqual("TooManyOpenDeposits", preview.Blocker);

            await Deposits.AttackAsync(7, light, Attack([Req("Ruby", "Polished", 10)])); // continua permitido
        }

        [TestMethod]
        public async Task Attack_AnOpenDeposit_PersistsIntoTheNextWeekAndCanStillBeAttacked()
        {
            int id = await SeedDepositAsync();
            await GiveStagedAsync(OreType.Ruby, ItemStage.Polished, 20);
            await Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Polished", 10)]));

            Time.Advance(TimeSpan.FromDays(7));
            var state = (await Players.GetStateAsync(7)).Deposits!;
            Assert.IsTrue(state.Items.Single(i => i.Id == id).CanAttack);

            var res = await Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Polished", 10)]));
            Assert.AreEqual(60, res.DefenseBefore);
            Assert.AreEqual(20, res.DefenseAfter);
        }

        [TestMethod]
        public async Task Attack_UntouchedFromPastWeek_IsExpired()
        {
            int id = await SeedDepositAsync();
            await GiveStagedAsync(OreType.Ruby, ItemStage.Polished, 10);
            Time.Advance(TimeSpan.FromDays(7));

            var ex = await ThrowsAsync(() => Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Polished", 10)])));

            Assert.AreEqual(ShardsErrorCode.DepositExpired, ex.Code);
        }

        [TestMethod]
        public async Task Attack_DepletedOrAlreadyBroken_IsRefused()
        {
            int id = await SeedDepositAsync();
            await GiveStagedAsync(OreType.Ruby, ItemStage.Polished, 10);

            await UpdateDepositAsync(id, d => { d.Status = DepositStatus.Open; d.DefenseCurrent = 0; });
            Assert.AreEqual(ShardsErrorCode.DepositDefenseBroken, (await ThrowsAsync(() => Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Polished", 1)])))).Code);

            await UpdateDepositAsync(id, d => { d.Status = DepositStatus.Depleted; d.StockRemaining = 0; });
            Assert.AreEqual(ShardsErrorCode.DepositDepleted, (await ThrowsAsync(() => Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Polished", 1)])))).Code);
        }

        [TestMethod]
        public async Task Attack_AnotherPlayersOrUnknownDeposit_IsNotFound()
        {
            int id = await SeedDepositAsync();
            await SeedPlayerAsync(8);
            await GiveStagedAsync(OreType.Ruby, ItemStage.Polished, 10, uid: 8);

            var other = await ThrowsAsync(() => Deposits.AttackAsync(8, id, Attack([Req("Ruby", "Polished", 1)])));
            var unknown = await ThrowsAsync(() => Deposits.AttackAsync(7, 9999, Attack([Req("Ruby", "Polished", 1)])));

            Assert.AreEqual(ShardsErrorCode.DepositNotFound, other.Code);
            Assert.AreEqual(ShardsErrorCode.DepositNotFound, unknown.Code);
            Assert.AreEqual(100, (await LoadDepositAsync(id)).DefenseCurrent);
        }

        [TestMethod]
        public async Task Attack_InvalidBody_IsRefusedBeforeTouchingAnything()
        {
            int id = await SeedDepositAsync();
            await GiveStagedAsync(OreType.Ruby, ItemStage.Polished, 10);

            Assert.AreEqual(ShardsErrorCode.InvalidGem, (await ThrowsAsync(() => Deposits.AttackAsync(7, id, Attack([Req("Quartz", "Polished", 1)])))).Code);
            Assert.AreEqual(ShardsErrorCode.InvalidGem, (await ThrowsAsync(() => Deposits.AttackAsync(7, id, Attack([Req("Nope", "Polished", 1)])))).Code);
            Assert.AreEqual(ShardsErrorCode.InvalidStage, (await ThrowsAsync(() => Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Raw", 1)])))).Code);
            Assert.AreEqual(ShardsErrorCode.InvalidStage, (await ThrowsAsync(() => Deposits.AttackAsync(7, id, Attack([Req("Ruby", null!, 1)])))).Code);
            Assert.AreEqual(ShardsErrorCode.InvalidAttack, (await ThrowsAsync(() => Deposits.AttackAsync(7, id, null))).Code);
            Assert.AreEqual(ShardsErrorCode.InvalidAttack, (await ThrowsAsync(() => Deposits.AttackAsync(7, id, Attack([])))).Code);
            Assert.AreEqual(ShardsErrorCode.InvalidAttack, (await ThrowsAsync(() => Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Polished", 0)])))).Code);
            Assert.AreEqual(ShardsErrorCode.InvalidAttack, (await ThrowsAsync(() => Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Polished", 1)], [Req("Sapphire", "Polished", -1)])))).Code);

            Assert.AreEqual(10, await StagedAsync(OreType.Ruby, ItemStage.Polished));
            Assert.AreEqual(100, (await LoadDepositAsync(id)).DefenseCurrent);
        }

        [TestMethod]
        public async Task Attack_TooManyStacks_IsRefused()
        {
            Balance.Deposits.MaxAttackStacks = 1;
            int id = await SeedDepositAsync();

            var ex = await ThrowsAsync(() => Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Polished", 1), Req("Ruby", "Faceted", 1)])));

            Assert.AreEqual(ShardsErrorCode.InvalidAttack, ex.Code);
        }

        [TestMethod]
        public async Task Attack_ThatDealsNoDamage_IsRefused_AndConsumesNothing()
        {
            Balance.Combat.Effectiveness.Add(new EffectivenessOptions { Attacker = Element.Fire, Defender = Element.Fire, Multiplier = 0 });
            int id = await SeedDepositAsync();
            await GiveStagedAsync(OreType.Ruby, ItemStage.Faceted, 5);

            var ex = await ThrowsAsync(() => Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Faceted", 5)])));

            Assert.AreEqual(ShardsErrorCode.InvalidAttack, ex.Code);
            Assert.AreEqual(5, await StagedAsync(OreType.Ruby, ItemStage.Faceted));
            Assert.AreEqual(Balance.Explore.MaxEnergy, await ExploreEnergyAsync());
        }

        [TestMethod]
        public async Task State_ShowsCanAttackCounterRatioAndTheEnergyConversion()
        {
            await SeedPlayerAsync();
            await SetWorkshopAsync(1);

            var state = await Players.GetStateAsync(7);

            Assert.AreEqual(3, state.Combat.EnergyPerCounterPoint, 1e-9);
            CollectionAssert.AreEqual(new[] { 0.05, 0.1, 0.2 }, state.Deposits!.Items.Select(i => i.CounterRatio).ToArray());
            Assert.IsTrue(state.Deposits.Items.All(i => i.CanAttack));
        }

        [TestMethod]
        public async Task Attack_SerializesTheContractTheFrontExpects()
        {
            int id = await SeedDepositAsync();
            await GiveStagedAsync(OreType.Ruby, ItemStage.Faceted, 4);
            await GiveStagedAsync(OreType.Sapphire, ItemStage.Polished, 2);

            var res = await Deposits.AttackAsync(7, id, Attack([Req("Ruby", "Faceted", 4)], [Req("Sapphire", "Polished", 2)], preview: true));
            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(res, Web));
            var root = doc.RootElement;

            Assert.IsTrue(root.GetProperty("preview").GetBoolean());
            Assert.IsTrue(root.GetProperty("canConfirm").GetBoolean());
            Assert.AreEqual(JsonValueKind.Null, root.GetProperty("blocker").ValueKind);
            Assert.AreEqual(100, root.GetProperty("damageDealt").GetInt32());
            Assert.AreEqual(0, root.GetProperty("defenseAfter").GetInt32());
            Assert.IsTrue(root.GetProperty("broken").GetBoolean());
            Assert.AreEqual("Ruby", root.GetProperty("attackersUsed")[0].GetProperty("oreType").GetString());
            Assert.AreEqual("Faceted", root.GetProperty("attackersUsed")[0].GetProperty("stage").GetString());
            Assert.AreEqual("Sapphire", root.GetProperty("defendersUsed")[0].GetProperty("oreType").GetString());
            Assert.AreEqual(5.0, root.GetProperty("counterAttack").GetDouble(), 1e-9);
            Assert.AreEqual(0, root.GetProperty("energyCost").GetInt32());
            Assert.AreEqual(Balance.Explore.MaxEnergy, root.GetProperty("exploreEnergyAfter").GetInt32());
            Assert.AreEqual("Sealed", root.GetProperty("deposit").GetProperty("status").GetString());
            Assert.IsTrue(root.GetProperty("deposits").GetProperty("items").GetArrayLength() == 3);
            Assert.IsTrue(root.GetProperty("inventory").GetProperty("items").GetArrayLength() >= 1);
            Assert.AreEqual(Balance.Explore.MaxEnergy, root.GetProperty("player").GetProperty("exploreEnergy").GetInt32());
        }
    }
}
