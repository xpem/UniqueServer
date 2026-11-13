using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Shards.Config;
using Shards.Errors;
using Shards.Model.DTO;
using Shards.Rules;
using Shards.Service;

namespace ShardsTests
{
    [TestClass]
    public class OreCatalogRulesTests : GameRulesTestBase
    {
        [TestMethod]
        public void EveryOreType_HasACatalogEntry_AndTheRealConfigIsValid()
        {
            foreach (OreType ore in Enum.GetValues<OreType>())
                Assert.IsNotNull(Rules.FindOre(ore), $"{ore} sem entrada no catálogo");

            var result = new GameBalanceOptionsValidator().Validate(null, Balance);
            Assert.IsTrue(result.Succeeded, string.Join("; ", result.Failures ?? []));
        }

        [TestMethod]
        public void Elements_AreAsDecided()
        {
            Assert.AreEqual(Element.Fire, Rules.ElementOf(OreType.Ruby));
            Assert.AreEqual(Element.Water, Rules.ElementOf(OreType.Sapphire));
            Assert.AreEqual(Element.Wind, Rules.ElementOf(OreType.Emerald));
            Assert.AreEqual(Element.Wind, Rules.ElementOf(OreType.Amethyst), "Vento, provisoriamente");
            Assert.AreEqual(Element.Neutral, Rules.ElementOf(OreType.Quartz));
        }

        [TestMethod]
        public void OnlyQuartzIsNeutral()
        {
            foreach (OreOptions ore in Balance.Ores.Where(o => o.Ore != OreType.Quartz))
                Assert.AreNotEqual(Element.Neutral, ore.Element, $"{ore.Ore} deveria ter elemento");

            Assert.AreEqual(OreKind.Currency, Rules.FindOre(OreType.Quartz)!.Kind);
            Assert.AreEqual(OreKind.Exclusive, Rules.FindOre(OreType.Amethyst)!.Kind);
        }

        [TestMethod]
        public void Gems_AreTheKindGemOres_AndExcludeQuartzAndAmethyst()
        {
            CollectionAssert.AreEquivalent(new[] { OreType.Ruby, OreType.Sapphire, OreType.Emerald }, Rules.GemOres.ToArray());
        }

        [TestMethod]
        public void Craftable_IsTheThreeGemsAndTheAmethyst()
        {
            CollectionAssert.AreEqual(new[] { OreType.Ruby, OreType.Sapphire, OreType.Emerald, OreType.Amethyst }, Rules.CraftableOres.ToArray());
            CollectionAssert.AreEqual(new[] { OreType.Ruby, OreType.Sapphire, OreType.Emerald }, Rules.GemOres.ToArray(), "a ametista é Exclusive: não entra em Collector nem FullSet");
            Assert.IsTrue(Rules.IsCraftable(OreType.Amethyst));
            Assert.IsFalse(Rules.IsCraftable(OreType.Quartz));
        }

        [TestMethod]
        [DataRow(OreType.Ruby, 4, 32, 2, 16)]
        [DataRow(OreType.Sapphire, 2, 16, 4, 32)]
        [DataRow(OreType.Emerald, 3, 24, 3, 24)]
        [DataRow(OreType.Amethyst, 1, 8, 5, 40)]
        public void Profiles_AttackAndDefenseByStage(OreType ore, int attackPolished, int attackFaceted, int defensePolished, int defenseFaceted)
        {
            Assert.AreEqual(attackPolished, Rules.AttackOf(ore, ItemStage.Polished), $"{ore}: ataque polida");
            Assert.AreEqual(attackFaceted, Rules.AttackOf(ore, ItemStage.Faceted), $"{ore}: ataque facetada");
            Assert.AreEqual(defensePolished, Rules.DefenseOf(ore, ItemStage.Polished), $"{ore}: defesa polida");
            Assert.AreEqual(defenseFaceted, Rules.DefenseOf(ore, ItemStage.Faceted), $"{ore}: defesa facetada");

            Assert.AreEqual(0, Rules.AttackOf(ore, ItemStage.Raw), "bruta não ataca");
            Assert.AreEqual(0, Rules.DefenseOf(ore, ItemStage.Raw), "bruta não defende");
        }

        [TestMethod]
        public void Profiles_RubyAttacks_SapphireDefends_AmethystIsTheMostDefensive()
        {
            Assert.IsTrue(Rules.AttackOf(OreType.Ruby, ItemStage.Polished) > Rules.DefenseOf(OreType.Ruby, ItemStage.Polished));
            Assert.IsTrue(Rules.DefenseOf(OreType.Sapphire, ItemStage.Polished) > Rules.AttackOf(OreType.Sapphire, ItemStage.Polished));
            Assert.AreEqual(Rules.AttackOf(OreType.Emerald, ItemStage.Polished), Rules.DefenseOf(OreType.Emerald, ItemStage.Polished));

            foreach (OreType gem in new[] { OreType.Ruby, OreType.Sapphire, OreType.Emerald })
                Assert.IsTrue(Rules.DefenseOf(OreType.Amethyst, ItemStage.Polished) > Rules.DefenseOf(gem, ItemStage.Polished), gem.ToString());
        }

        [TestMethod]
        public void Quartz_NeitherAttacksNorDefends()
        {
            Assert.AreEqual(0, Rules.AttackOf(OreType.Quartz, ItemStage.Polished));
            Assert.AreEqual(0, Rules.DefenseOf(OreType.Quartz, ItemStage.Faceted));
        }

        [TestMethod]
        [DataRow(Element.Fire, Element.Wind, 1.5)]
        [DataRow(Element.Wind, Element.Water, 1.5)]
        [DataRow(Element.Water, Element.Fire, 1.5)]
        [DataRow(Element.Wind, Element.Fire, 0.75)]
        [DataRow(Element.Water, Element.Wind, 0.75)]
        [DataRow(Element.Fire, Element.Water, 0.75)]
        [DataRow(Element.Fire, Element.Fire, 1.0)]
        [DataRow(Element.Wind, Element.Wind, 1.0)]
        [DataRow(Element.Neutral, Element.Fire, 1.0)]
        [DataRow(Element.Fire, Element.Neutral, 1.0)]
        [DataRow(Element.Neutral, Element.Neutral, 1.0)]
        public void Effectiveness_ComesFromTheMatrix(Element attacker, Element defender, double expected)
        {
            Assert.AreEqual(expected, Rules.ElementMultiplier(attacker, defender), 1e-9);
        }

        [TestMethod]
        public void Matrix_HasOppositeEffects_ForEachPairOfElements()
        {
            var elements = new[] { Element.Fire, Element.Wind, Element.Water };

            foreach (var a in elements)
                foreach (var d in elements.Where(x => x != a))
                {
                    double forward = Rules.ElementMultiplier(a, d);
                    double backward = Rules.ElementMultiplier(d, a);
                    Assert.IsTrue((forward == 1.5 && backward == 0.75) || (forward == 0.75 && backward == 1.5), $"{a} x {d}");
                }
        }

        [TestMethod]
        public void EmptyMatrix_GivesNoAdvantageToAnyone()
        {
            Balance.Combat.Effectiveness = [];

            Assert.AreEqual(1.0, Rules.ElementMultiplier(Element.Fire, Element.Wind), 1e-9);
            Assert.AreEqual(1.0, Rules.ElementMultiplier(Element.Wind, Element.Fire), 1e-9);
        }

        [TestMethod]
        public void Matrix_SupportsImmunityAndDoubleEffect_AndNewElementsAreJustConfiguration()
        {
            Balance.Combat.Effectiveness =
            [
                new EffectivenessOptions { Attacker = Element.Fire, Defender = Element.Water, Multiplier = 0 },
                new EffectivenessOptions { Attacker = Element.Water, Defender = Element.Fire, Multiplier = 2 },
            ];

            Assert.AreEqual(0.0, Rules.ElementMultiplier(Element.Fire, Element.Water), 1e-9, "imune");
            Assert.AreEqual(2.0, Rules.ElementMultiplier(Element.Water, Element.Fire), 1e-9, "efeito dobrado");
            Assert.AreEqual(1.0, Rules.ElementMultiplier(Element.Fire, Element.Wind), 1e-9, "par ausente");
        }
    }

    [TestClass]
    public class OreCatalogValidatorTests : GameRulesTestBase
    {
        private readonly GameBalanceOptionsValidator _validator = new();

        private IEnumerable<string> Failures => _validator.Validate(null, Balance).Failures ?? [];

        [TestMethod]
        public void MissingEntryForAnOreType_IsInvalid()
        {
            Balance.Ores.RemoveAll(o => o.Ore == OreType.Amethyst);

            Assert.IsTrue(Failures.Any(f => f.Contains("falta a entrada de Amethyst")));
        }

        [TestMethod]
        public void DuplicatedEntry_IsInvalid()
        {
            Balance.Ores.Add(new OreOptions { Ore = OreType.Ruby, Kind = OreKind.Gem, Element = Element.Fire });

            Assert.IsTrue(Failures.Any(f => f.Contains("Ruby está repetido")));
        }

        [TestMethod]
        public void QuartzMustBeTheOnlyCurrency()
        {
            Balance.Ores.Single(o => o.Ore == OreType.Quartz).Kind = OreKind.Gem;
            Assert.IsTrue(Failures.Any(f => f.Contains("Quartz deve ser do tipo Currency")));

            Balance.Ores.Single(o => o.Ore == OreType.Quartz).Kind = OreKind.Currency;
            Balance.Ores.Single(o => o.Ore == OreType.Amethyst).Kind = OreKind.Currency;
            Assert.IsTrue(Failures.Any(f => f.Contains("só pode haver uma moeda")));
        }

        [TestMethod]
        public void CurrencyCannotHaveElementAttackDefenseOrBeCrafted()
        {
            var quartz = Balance.Ores.Single(o => o.Ore == OreType.Quartz);
            quartz.Element = Element.Fire;
            quartz.Craftable = true;
            quartz.Attack[ItemStage.Polished] = 1;
            quartz.Defense[ItemStage.Polished] = 1;

            Assert.AreEqual(4, Failures.Count(f => f.Contains("Ores[Quartz]")));
        }

        [TestMethod]
        public void RawStageCannotAttackOrDefend()
        {
            var ruby = Balance.Ores.Single(o => o.Ore == OreType.Ruby);
            ruby.Attack[ItemStage.Raw] = 1;
            ruby.Defense[ItemStage.Raw] = 1;

            Assert.IsTrue(Failures.Any(f => f.Contains("Raw não ataca")));
            Assert.IsTrue(Failures.Any(f => f.Contains("Raw não defende")));
        }

        [TestMethod]
        public void PolishedAndFaceted_NeedAttackAndDefenseAboveZero()
        {
            var ruby = Balance.Ores.Single(o => o.Ore == OreType.Ruby);

            ruby.Attack.Remove(ItemStage.Faceted);
            Assert.IsTrue(Failures.Any(f => f.Contains("Ores[Ruby]") && f.Contains("Attack") && f.Contains("Faceted")));

            ruby.Attack[ItemStage.Faceted] = 32;
            ruby.Defense[ItemStage.Polished] = 0;
            Assert.IsTrue(Failures.Any(f => f.Contains("Ores[Ruby]") && f.Contains("Defense") && f.Contains("Polished")));
        }

        [TestMethod]
        public void OnlyQuartzMayBeNeutral()
        {
            Balance.Ores.Single(o => o.Ore == OreType.Amethyst).Element = Element.Neutral;

            Assert.IsTrue(Failures.Any(f => f.Contains("Ores[Amethyst]") && f.Contains("só o quartzo")));
        }

        [TestMethod]
        public void OreElement_MustAppearInTheMatrix()
        {
            Balance.Combat.Effectiveness.RemoveAll(e => e.Attacker == Element.Fire || e.Defender == Element.Fire);

            Assert.IsTrue(Failures.Any(f => f.Contains("Ores[Ruby]") && f.Contains("Fire precisa aparecer em Combat.Effectiveness")));
        }

        [TestMethod]
        public void ExclusiveOre_CannotBeAMineProduction()
        {
            Balance.Productions.Add(new ProductionOptions
            {
                Focus = OreType.Amethyst,
                Drops = [new DropOptions { Ore = OreType.Amethyst, Chance = 1, Xp = 1 }],
            });

            Assert.IsTrue(Failures.Any(f => f.Contains("exclusivo") && f.Contains("tipo de produção")));
        }

        [TestMethod]
        public void Matrix_RejectsNeutral_NegativeMultiplier_AndRepeatedPairs()
        {
            Balance.Combat.Effectiveness.Add(new EffectivenessOptions { Attacker = Element.Neutral, Defender = Element.Fire, Multiplier = 1 });
            Assert.IsTrue(Failures.Any(f => f.Contains("Neutral não entra na matriz")));

            Balance.Combat.Effectiveness.RemoveAll(e => e.Attacker == Element.Neutral);
            Balance.Combat.Effectiveness.Add(new EffectivenessOptions { Attacker = Element.Fire, Defender = Element.Fire, Multiplier = -1 });
            Assert.IsTrue(Failures.Any(f => f.Contains("não pode ser negativo")));

            Balance.Combat.Effectiveness.RemoveAll(e => e.Attacker == Element.Fire && e.Defender == Element.Fire);
            Balance.Combat.Effectiveness.Add(new EffectivenessOptions { Attacker = Element.Fire, Defender = Element.Wind, Multiplier = 2 });
            Assert.IsTrue(Failures.Any(f => f.Contains("está repetido") && f.Contains("Fire x Wind")));
        }

        [TestMethod]
        public void Matrix_AllowsZeroForImmunity()
        {
            Balance.Combat.Effectiveness.Single(e => e.Attacker == Element.Fire && e.Defender == Element.Water).Multiplier = 0;

            Assert.IsFalse(Failures.Any(f => f.Contains("Combat.Effectiveness")));
        }
    }

    [TestClass]
    public class OreCatalogStateTests : ShardsServiceTestBase
    {
        [TestMethod]
        public async Task State_ExposesTheCatalogAndTheEffectivenessMatrix()
        {
            var state = await Players.GetStateAsync(7);

            CollectionAssert.AreEqual(
                new[] { OreType.Quartz, OreType.Ruby, OreType.Sapphire, OreType.Emerald, OreType.Amethyst },
                state.Ores.Select(o => o.OreType).ToArray());

            var ruby = state.Ores.Single(o => o.OreType == OreType.Ruby);
            Assert.AreEqual(OreKind.Gem, ruby.Kind);
            Assert.AreEqual(Element.Fire, ruby.Element);
            Assert.IsTrue(ruby.Craftable);
            Assert.AreEqual(4, ruby.Attack.Single(a => a.Stage == ItemStage.Polished).Attack);
            Assert.AreEqual(32, ruby.Attack.Single(a => a.Stage == ItemStage.Faceted).Attack);
            Assert.AreEqual(2, ruby.Defense.Single(d => d.Stage == ItemStage.Polished).Defense);
            Assert.AreEqual(16, ruby.Defense.Single(d => d.Stage == ItemStage.Faceted).Defense);

            var amethyst = state.Ores.Single(o => o.OreType == OreType.Amethyst);
            Assert.AreEqual(OreKind.Exclusive, amethyst.Kind);
            Assert.AreEqual(Element.Wind, amethyst.Element);
            Assert.IsTrue(amethyst.Craftable);
            Assert.AreEqual(5, amethyst.Defense.Single(d => d.Stage == ItemStage.Polished).Defense);

            var quartz = state.Ores.Single(o => o.OreType == OreType.Quartz);
            Assert.AreEqual(0, quartz.Attack.Count);
            Assert.AreEqual(0, quartz.Defense.Count);

            Assert.AreEqual(6, state.Combat.Effectiveness.Count);
            var fireVsWind = state.Combat.Effectiveness.Single(e => e.Attacker == Element.Fire && e.Defender == Element.Wind);
            Assert.AreEqual(1.5, fireVsWind.Multiplier, 1e-9);
            var windVsFire = state.Combat.Effectiveness.Single(e => e.Attacker == Element.Wind && e.Defender == Element.Fire);
            Assert.AreEqual(0.75, windVsFire.Multiplier, 1e-9);
        }

        [TestMethod]
        public async Task Workshop_ListsTheThreeGemsAndTheAmethyst()
        {
            var workshop = (await Players.GetStateAsync(7)).Workshop;

            foreach (var recipe in workshop)
                CollectionAssert.AreEqual(new[] { OreType.Ruby, OreType.Sapphire, OreType.Emerald, OreType.Amethyst }, recipe.Gems.Select(g => g.OreType).ToArray());
        }

        [TestMethod]
        public async Task Crafting_Amethyst_IsAllowedNow_AndPolishesIt()
        {
            await SeedPlayerAsync();
            await using (var ctx = Db.CreateDbContext())
            {
                var player = await ctx.Player.SingleAsync(p => p.UserId == 7);
                ctx.PlayerSkill.Add(new PlayerSkillDTO { PlayerId = player.Id, SkillType = SkillType.Workshop, Level = 1 });
                await ctx.SaveChangesAsync();
            }
            await GiveOresAsync(OreType.Amethyst, 5);
            await GiveOresAsync(OreType.Quartz, 100);
            var workshop = new WorkshopService(Db, Rules, new ShardsMapper(Rules));

            var res = await workshop.CraftAsync(7, "Amethyst", "Polished", 1);

            Assert.AreEqual(OreType.Amethyst, res.Crafted.OreType);
            Assert.AreEqual(ItemStage.Polished, res.Crafted.Stage);
            Assert.AreEqual(1, res.Crafted.Quantity);
        }

        [TestMethod]
        public void Collector_DoesNotNeedAmethyst()
        {
            // a ametista é Exclusive: não entra nas missões de coleção (Collector, FullSet)
            var facts = new MissionFacts(
                Inventory: [new InventoryItemDTO { PlayerId = 1, OreType = OreType.Ruby }, new InventoryItemDTO { PlayerId = 1, OreType = OreType.Sapphire }, new InventoryItemDTO { PlayerId = 1, OreType = OreType.Emerald }],
                Gems: Rules.GemOres);

            Assert.IsTrue(MissionTracker.IsSatisfied(MissionType.Collector, facts));
        }

        [TestMethod]
        public void Collector_IsNeverSatisfied_WhenTheGemListIsMissing()
        {
            var facts = new MissionFacts(Inventory: [new InventoryItemDTO { PlayerId = 1, OreType = OreType.Ruby }]);

            Assert.IsFalse(MissionTracker.IsSatisfied(MissionType.Collector, facts));
            Assert.IsFalse(MissionTracker.IsSatisfied(MissionType.FullSet, facts));
        }

        [TestMethod]
        public void Amethyst_SerializesByName()
        {
            string json = JsonSerializer.Serialize(new Shards.Model.Res.OreAmountRes { OreType = OreType.Amethyst, Quantity = 1 }, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            using var doc = JsonDocument.Parse(json);
            Assert.AreEqual("Amethyst", doc.RootElement.GetProperty("oreType").GetString());
        }
    }
}
