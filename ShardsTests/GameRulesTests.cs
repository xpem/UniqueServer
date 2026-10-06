using Shards.Model.DTO;

namespace ShardsTests
{
    [TestClass]
    public class EnergyRegenerationTests : GameRulesTestBase
    {
        [TestMethod]
        public void FullEnergy_ResetsTimestampToNow()
        {
            Time.Advance(TimeSpan.FromHours(10));

            var state = Rules.RegenerateEnergy(100, 100, Start);

            Assert.AreEqual(100, state.Energy);
            Assert.AreEqual(Time.Now.UtcDateTime, state.LastUpdateUtc);
        }

        [TestMethod]
        public void LessThanOnePoint_DoesNotChangeAnything()
        {
            Time.Advance(TimeSpan.FromSeconds(215));

            var state = Rules.RegenerateEnergy(50, 100, Start);

            Assert.AreEqual(50, state.Energy);
            Assert.AreEqual(Start, state.LastUpdateUtc);
        }

        [TestMethod]
        public void PartialRemainder_IsPreserved()
        {
            // 500 s = 2 pontos (432 s) + 68 s de resto
            Time.Advance(TimeSpan.FromSeconds(500));

            var state = Rules.RegenerateEnergy(10, 100, Start);

            Assert.AreEqual(12, state.Energy);
            Assert.AreEqual(Start.AddSeconds(432), state.LastUpdateUtc);
        }

        [TestMethod]
        public void RepeatedChecks_DoNotLoseOrCreateEnergy()
        {
            // consultar a cada 100 s durante 432 s totais deve render exatamente 2 pontos
            int energy = 10;
            DateTime last = Start;

            for (int i = 0; i < 4; i++)
            {
                Time.Advance(TimeSpan.FromSeconds(100));
                (energy, last) = Rules.RegenerateEnergy(energy, 100, last);
            }
            Time.Advance(TimeSpan.FromSeconds(32));
            (energy, last) = Rules.RegenerateEnergy(energy, 100, last);

            Assert.AreEqual(12, energy);
        }

        [TestMethod]
        public void EnoughTimeToFill_CapsAtMaxAndStartsFromNow()
        {
            Time.Advance(TimeSpan.FromHours(6));

            var state = Rules.RegenerateEnergy(0, 100, Start);

            Assert.AreEqual(100, state.Energy);
            Assert.AreEqual(Time.Now.UtcDateTime, state.LastUpdateUtc);
        }

        [TestMethod]
        public void ClockSkew_FutureTimestamp_DoesNotRegenerate()
        {
            var state = Rules.RegenerateEnergy(20, 100, Start.AddMinutes(5));

            Assert.AreEqual(20, state.Energy);
            Assert.AreEqual(Start.AddMinutes(5), state.LastUpdateUtc);
        }

        [TestMethod]
        public void NextEnergyAt_IsNullWhenFull_OtherwiseOnePointAhead()
        {
            Assert.IsNull(Rules.NextEnergyAtUtc(100, 100, Start));
            Assert.AreEqual(Start.AddSeconds(216), Rules.NextEnergyAtUtc(99, 100, Start));
        }
    }

    [TestClass]
    public class ExperienceTests : GameRulesTestBase
    {
        [TestMethod]
        public void Curve_MatchesPlan()
        {
            int[] expected = [0, 15, 45, 90, 150, 225];

            for (int level = 0; level < expected.Length; level++)
                Assert.AreEqual(expected[level], Rules.XpForLevel(level), $"nível {level}");
        }

        [TestMethod]
        public void LevelForXp_RespectsThresholds()
        {
            Assert.AreEqual(0, Rules.LevelForXp(0));
            Assert.AreEqual(0, Rules.LevelForXp(14));
            Assert.AreEqual(1, Rules.LevelForXp(15));
            Assert.AreEqual(1, Rules.LevelForXp(44));
            Assert.AreEqual(2, Rules.LevelForXp(45));
            Assert.AreEqual(5, Rules.LevelForXp(225));
        }

        [TestMethod]
        public void AddExperience_CanGainMoreThanOneLevel()
        {
            var result = Rules.AddExperience(0, 10, 90);

            Assert.AreEqual(100, result.Experience);
            Assert.AreEqual(3, result.Level);
            Assert.AreEqual(3, result.LevelsGained);
        }

        [TestMethod]
        public void AddExperience_WithoutCrossingThreshold_KeepsLevel()
        {
            var result = Rules.AddExperience(0, 5, 5);

            Assert.AreEqual(0, result.Level);
            Assert.AreEqual(0, result.LevelsGained);
            Assert.AreEqual(10, result.Experience);
        }

        [TestMethod]
        public void XpToNextLevel_IsDistanceToNextThreshold()
        {
            Assert.AreEqual(15, Rules.XpToNextLevel(0, 0));
            Assert.AreEqual(5, Rules.XpToNextLevel(0, 10));
            Assert.AreEqual(30, Rules.XpToNextLevel(1, 15));
        }
    }

    [TestClass]
    public class CartTests : GameRulesTestBase
    {
        private const double Rate = 0.33;

        [TestMethod]
        public void JustCollected_IsEmpty()
        {
            Assert.AreEqual(0, Rules.CartAmount(100, Rate, Start));
        }

        [TestMethod]
        public void IsRoundedDown()
        {
            Time.Advance(TimeSpan.FromMinutes(10)); // 3,3 minérios

            Assert.AreEqual(3, Rules.CartAmount(100, Rate, Start));
        }

        [TestMethod]
        public void FloatingPoint_DoesNotLoseAnOre()
        {
            Time.Advance(TimeSpan.FromMinutes(100)); // 100 * 0.33 = 33 exatos

            Assert.AreEqual(33, Rules.CartAmount(100, Rate, Start));
        }

        [TestMethod]
        public void IsCappedAtCapacity()
        {
            Time.Advance(TimeSpan.FromDays(3));

            Assert.AreEqual(100, Rules.CartAmount(100, Rate, Start));
        }

        [TestMethod]
        public void ZeroRate_GeneratesNothing()
        {
            Time.Advance(TimeSpan.FromDays(1));

            Assert.AreEqual(0, Rules.CartAmount(100, 0, Start));
            Assert.IsNull(Rules.CartFullAtUtc(100, 0, Start));
        }

        [TestMethod]
        public void FutureTimestamp_IsEmpty()
        {
            Assert.AreEqual(0, Rules.CartAmount(100, Rate, Start.AddHours(1)));
        }

        [TestMethod]
        public void FullAt_IsCapacityOverRate()
        {
            DateTime? fullAt = Rules.CartFullAtUtc(100, Rate, Start);

            Assert.IsNotNull(fullAt);
            Assert.AreEqual(100 / Rate, (fullAt.Value - Start).TotalMinutes, 1e-6);
        }
    }

    [TestClass]
    public class DropAndSkillTests : GameRulesTestBase
    {
        [TestMethod]
        public void RollDrop_LowRollIsBronze_HighRollIsSilver()
        {
            var mine = Rules.GetMine(1);

            Random.Value = 0.0;
            Assert.AreEqual(OreType.Bronze, Rules.RollDrop(mine).Ore);

            Random.Value = 0.849;
            Assert.AreEqual(OreType.Bronze, Rules.RollDrop(mine).Ore);

            Random.Value = 0.85;
            Assert.AreEqual(OreType.Silver, Rules.RollDrop(mine).Ore);

            Random.Value = 0.9999999;
            Assert.AreEqual(OreType.Silver, Rules.RollDrop(mine).Ore);
        }

        [TestMethod]
        public void RollDrop_GivesConfiguredXp()
        {
            var mine = Rules.GetMine(1);

            Random.Value = 0.0;
            Assert.AreEqual(1, Rules.RollDrop(mine).Xp);

            Random.Value = 0.9;
            Assert.AreEqual(3, Rules.RollDrop(mine).Xp);
        }

        [TestMethod]
        public void DoubleDropChance_ScalesWithLevelAndIsCapped()
        {
            Assert.AreEqual(0.0, Rules.DoubleDropChance(0), 1e-9);
            Assert.AreEqual(0.15, Rules.DoubleDropChance(1), 1e-9);
            Assert.AreEqual(0.30, Rules.DoubleDropChance(2), 1e-9);
            Assert.AreEqual(0.75, Rules.DoubleDropChance(5), 1e-9);
            Assert.AreEqual(0.75, Rules.DoubleDropChance(99), 1e-9, "acima do nível máximo não passa de 5 níveis");
            Assert.AreEqual(0.0, Rules.DoubleDropChance(-3), 1e-9);
        }

        [TestMethod]
        public void RollDoubleDrop_UsesChance()
        {
            Random.Value = 0.10;
            Assert.IsTrue(Rules.RollDoubleDrop(1));

            Random.Value = 0.20;
            Assert.IsFalse(Rules.RollDoubleDrop(1));

            Random.Value = 0.0;
            Assert.IsFalse(Rules.RollDoubleDrop(0), "sem a skill nunca há drop duplo");
        }
    }

    [TestClass]
    public class MinesAndConfigTests : GameRulesTestBase
    {
        [TestMethod]
        public void AppSettings_BalanceBindsWithoutDuplicates()
        {
            Assert.AreEqual(2, Balance.Mines.Count);
            Assert.AreEqual(2, Balance.Mines[0].Drops.Count);
            Assert.AreEqual(0, Balance.Mines[0].UnlockCost.Count);
            Assert.AreEqual(2, Balance.Mines[1].UnlockCost.Count);
        }

        [TestMethod]
        public void AppSettings_MatchesPlanValues()
        {
            Assert.AreEqual(100, Balance.Energy.MaxEnergy);
            Assert.AreEqual(216, Balance.Energy.SecondsPerPoint);
            Assert.AreEqual(15, Balance.Experience.XpFirstLevel);

            var mine1 = Rules.GetMine(1);
            Assert.AreEqual(100, mine1.CartCapacity);
            Assert.AreEqual(0.33, mine1.OresPerMinute, 1e-9);
            Assert.AreEqual(1.0, mine1.Drops.Sum(d => d.Chance), 1e-9);

            var cost = Rules.GetMine(2).UnlockCost;
            Assert.AreEqual(150, cost.Single(c => c.Ore == OreType.Bronze).Quantity);
            Assert.AreEqual(30, cost.Single(c => c.Ore == OreType.Silver).Quantity);
        }

        [TestMethod]
        public void GetNextMine_FollowsHighestOwned()
        {
            Assert.AreEqual(2, Rules.GetNextMine(1)?.MineNumber);
            Assert.IsNull(Rules.GetNextMine(2));
        }

        [TestMethod]
        public void GetMine_Unknown_Throws()
        {
            Assert.ThrowsExactly<KeyNotFoundException>(() => Rules.GetMine(99));
        }
    }

    [TestClass]
    public class MissionCatalogTests
    {
        [TestMethod]
        public void Chain_FollowsPlanOrder()
        {
            Assert.AreEqual(MissionType.FirstPickaxe, Shards.Rules.MissionCatalog.First.Type);
            Assert.AreEqual(MissionType.Specialization, Shards.Rules.MissionCatalog.Next(MissionType.FirstPickaxe)?.Type);
            Assert.AreEqual(MissionType.OperationalExpansion, Shards.Rules.MissionCatalog.Next(MissionType.Specialization)?.Type);
            Assert.IsNull(Shards.Rules.MissionCatalog.Next(MissionType.OperationalExpansion));
        }

        [TestMethod]
        public void EveryMissionType_HasDefinition()
        {
            foreach (MissionType type in Enum.GetValues<MissionType>())
                Assert.AreEqual(type, Shards.Rules.MissionCatalog.Get(type).Type);
        }

        [TestMethod]
        public void Goals_AndRewards_MatchPlan()
        {
            var m1 = Shards.Rules.MissionCatalog.Get(MissionType.FirstPickaxe);
            Assert.AreEqual(10, m1.Goal);
            Assert.AreEqual(15, m1.Reward.Xp);

            var m2 = Shards.Rules.MissionCatalog.Get(MissionType.Specialization);
            Assert.AreEqual(20, m2.Reward.Ores.Single(o => o.Ore == OreType.Bronze).Quantity);
            Assert.AreEqual(5, m2.Reward.Ores.Single(o => o.Ore == OreType.Silver).Quantity);
        }
    }
}
