using Shards.Config;
using Shards.Errors;
using Shards.Model.DTO;
using Shards.Service;

namespace ShardsTests
{
    [TestClass]
    public class EnumParsingHardeningTests
    {
        [TestMethod]
        [DataRow("FirstPickaxe,Specialization")]
        [DataRow("FirstPickaxe, Specialization")]
        [DataRow("Specialization,OperationalExpansion")]
        [DataRow("FirstPickaxe|Specialization")]
        [DataRow("First Pickaxe")]
        [DataRow("+1")]
        [DataRow("1")]
        public void ParseMissionType_RejectsListsNumbersAndSymbols(string value)
        {
            var ex = Assert.ThrowsExactly<ShardsException>(() => ShardsErrors.ParseMissionType(value));

            Assert.AreEqual(ShardsErrorCode.MissionNotFound, ex.Code);
        }

        [TestMethod]
        [DataRow("DoubleDrop,DoubleDrop")]
        [DataRow("DoubleDrop, DoubleDrop")]
        [DataRow("Double-Drop")]
        public void ParseSkillType_RejectsLists(string value)
        {
            var ex = Assert.ThrowsExactly<ShardsException>(() => ShardsErrors.ParseSkillType(value));

            Assert.AreEqual(ShardsErrorCode.InvalidSkill, ex.Code);
        }

        [TestMethod]
        public void ParseMissionType_StillAcceptsNamesIgnoringCaseAndSpaces()
        {
            Assert.AreEqual(MissionType.OperationalExpansion, ShardsErrors.ParseMissionType("  operationalexpansion "));
        }
    }

    [TestClass]
    public class GameBalanceOptionsValidatorTests : GameRulesTestBase
    {
        private readonly GameBalanceOptionsValidator _validator = new();

        [TestMethod]
        public void RealAppSettings_AreValid()
        {
            var result = _validator.Validate(null, Balance);

            Assert.IsTrue(result.Succeeded, string.Join("; ", result.Failures ?? []));
        }

        [TestMethod]
        public void EmptyMines_IsInvalid()
        {
            Balance.Mines.Clear();

            var result = _validator.Validate(null, Balance);

            Assert.IsTrue(result.Failed);
            Assert.IsTrue(result.Failures!.Any(f => f.Contains("Mines não pode ser vazio")));
        }

        [TestMethod]
        public void MissingFirstMine_IsInvalid()
        {
            Balance.Mines.RemoveAll(m => m.MineNumber == 1);

            Assert.IsTrue(_validator.Validate(null, Balance).Failures!.Any(f => f.Contains("MineNumber 1")));
        }

        [TestMethod]
        public void DuplicatedMineNumber_IsInvalid()
        {
            Balance.Mines.Add(new MineOptions { MineNumber = 2, Drops = [new DropOptions { Ore = OreType.Bronze, Chance = 1 }] });

            Assert.IsTrue(_validator.Validate(null, Balance).Failures!.Any(f => f.Contains("repetido")));
        }

        [TestMethod]
        public void MineWithoutDrops_IsInvalid()
        {
            Balance.Mines[0].Drops.Clear();

            Assert.IsTrue(_validator.Validate(null, Balance).Failures!.Any(f => f.Contains("Drops não pode ser vazio")));
        }

        [TestMethod]
        public void ZeroSecondsPerPoint_IsInvalid()
        {
            Balance.Energy.SecondsPerPoint = 0;

            Assert.IsTrue(_validator.Validate(null, Balance).Failures!.Any(f => f.Contains("SecondsPerPoint")));
        }

        [TestMethod]
        public void ChanceOutsideRange_IsInvalid()
        {
            Balance.DoubleDrop.ChancePerLevel = 1.5;

            Assert.IsTrue(_validator.Validate(null, Balance).Failures!.Any(f => f.Contains("ChancePerLevel")));
        }

        [TestMethod]
        public void NonPositiveUnlockCost_IsInvalid()
        {
            Balance.Mines[1].UnlockCost[0].Quantity = 0;

            Assert.IsTrue(_validator.Validate(null, Balance).Failures!.Any(f => f.Contains("UnlockCost")));
        }

        [TestMethod]
        public void MultipleProblems_AreAllReported()
        {
            Balance.Energy.MaxEnergy = 0;
            Balance.Energy.CostPerMine = 0;
            Balance.Experience.XpFirstLevel = 0;

            Assert.AreEqual(3, _validator.Validate(null, Balance).Failures!.Count());
        }
    }

    [TestClass]
    public class ExperienceOverflowTests : GameRulesTestBase
    {
        [TestMethod]
        public void LevelForXp_AtIntMax_Terminates()
        {
            int level = Rules.LevelForXp(int.MaxValue);

            Assert.IsTrue(level > 1000);
            Assert.IsTrue(Rules.XpForLevel(level) <= int.MaxValue);
            Assert.IsTrue(Rules.XpForLevel(level + 1) > int.MaxValue);
        }

        [TestMethod]
        public void AddExperience_SaturatesInsteadOfWrapping()
        {
            var result = Rules.AddExperience(0, int.MaxValue - 5, 100);

            Assert.AreEqual(int.MaxValue, result.Experience);
            Assert.IsTrue(result.Level > 0);
        }

        [TestMethod]
        public void XpToNextLevel_NeverNegative()
        {
            Assert.AreEqual(0, Rules.XpToNextLevel(0, int.MaxValue));
        }

        [TestMethod]
        public void XpForLevel_StillMatchesTheCurve()
        {
            Assert.AreEqual(225L, Rules.XpForLevel(5));
        }
    }

    [TestClass]
    public class MissionTrackerAdvanceChangedTests
    {
        private static readonly DateTime Now = new(2026, 10, 6, 12, 0, 0, DateTimeKind.Utc);

        [TestMethod]
        public void ActiveMission_IsReturnedInAList()
        {
            var mission = new PlayerMissionDTO { PlayerId = 1, MissionType = MissionType.FirstPickaxe };

            var changed = MissionTracker.AdvanceChanged([mission], MissionType.FirstPickaxe, 3, Now);

            Assert.AreEqual(1, changed.Count);
            Assert.AreEqual(3, changed[0].Progress);
        }

        [TestMethod]
        public void MissingOrInactiveMission_ReturnsEmptyList()
        {
            var claimed = new PlayerMissionDTO { PlayerId = 1, MissionType = MissionType.FirstPickaxe, Status = MissionStatus.Claimed };

            Assert.AreEqual(0, MissionTracker.AdvanceChanged([], MissionType.FirstPickaxe, 1, Now).Count);
            Assert.AreEqual(0, MissionTracker.AdvanceChanged([claimed], MissionType.FirstPickaxe, 1, Now).Count);
        }
    }

    [TestClass]
    public class UniqueViolationDetectionTests
    {
        [TestMethod]
        public void OnlyUniqueViolations_AreDetected()
        {
            var unique = new Microsoft.EntityFrameworkCore.DbUpdateException("x",
                new Npgsql.PostgresException("dup", "ERROR", "ERROR", Npgsql.PostgresErrorCodes.UniqueViolation));
            var foreignKey = new Microsoft.EntityFrameworkCore.DbUpdateException("x",
                new Npgsql.PostgresException("fk", "ERROR", "ERROR", Npgsql.PostgresErrorCodes.ForeignKeyViolation));

            Assert.IsTrue(ConcurrencyRetry.IsUniqueViolation(unique));
            Assert.IsFalse(ConcurrencyRetry.IsUniqueViolation(foreignKey));
            Assert.IsFalse(ConcurrencyRetry.IsUniqueViolation(new InvalidOperationException()));
        }
    }
}
