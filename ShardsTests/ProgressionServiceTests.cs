using Microsoft.EntityFrameworkCore;
using Shards.Errors;
using Shards.Model.DTO;

namespace ShardsTests
{
    [TestClass]
    public class DistributeSkillTests : ShardsServiceTestBase
    {
        [TestMethod]
        public async Task WithoutPoints_IsNoSkillPoints()
        {
            await SeedPlayerAsync();

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Skills.DistributeAsync(7, "DoubleDrop"));

            Assert.AreEqual(ShardsErrorCode.NoSkillPoints, ex.Code);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("Nope")]
        [DataRow("1")]
        public async Task InvalidSkill_IsRejectedBeforeAnything(string? value)
        {
            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Skills.DistributeAsync(7, value));

            Assert.AreEqual(ShardsErrorCode.InvalidSkill, ex.Code);
        }

        [TestMethod]
        public async Task UnknownPlayer_IsPlayerNotFound()
        {
            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Skills.DistributeAsync(99, "DoubleDrop"));

            Assert.AreEqual(ShardsErrorCode.PlayerNotFound, ex.Code);
        }

        [TestMethod]
        public async Task SpendsOnePoint_AndRaisesTheSkill()
        {
            await SeedPlayerAsync();
            await SetSkillPointsAsync(2);

            var res = await Skills.DistributeAsync(7, "doubledrop");

            Assert.AreEqual(1, res.SkillPoints);
            Assert.AreEqual(1, res.Skills.Single(s => s.Type == SkillType.DoubleDrop).Level);
            Assert.AreEqual(5, res.Skills.Single(s => s.Type == SkillType.DoubleDrop).MaxLevel);
            Assert.AreEqual(1, (await PlayerRow()).SkillPoints);
        }

        [TestMethod]
        public async Task SecondPoint_RaisesTheSameSkillRow()
        {
            await SeedPlayerAsync();
            await SetSkillPointsAsync(2);

            await Skills.DistributeAsync(7, "DoubleDrop");
            var res = await Skills.DistributeAsync(7, "DoubleDrop");

            Assert.AreEqual(2, res.Skills.Single(s => s.Type == SkillType.DoubleDrop).Level);
            Assert.AreEqual(0, res.SkillPoints);
            await using var ctx = Db.CreateDbContext();
            Assert.AreEqual(1, await ctx.PlayerSkill.CountAsync());
        }

        [TestMethod]
        public async Task AtMaxLevel_IsSkillMaxLevel_AndKeepsThePoint()
        {
            int playerId = await SeedPlayerAsync();
            await SetSkillPointsAsync(1);
            await using (var ctx = Db.CreateDbContext())
            {
                ctx.PlayerSkill.Add(new PlayerSkillDTO { PlayerId = playerId, SkillType = SkillType.DoubleDrop, Level = 5 });
                await ctx.SaveChangesAsync();
            }

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Skills.DistributeAsync(7, "DoubleDrop"));

            Assert.AreEqual(ShardsErrorCode.SkillMaxLevel, ex.Code);
            Assert.AreEqual(1, (await PlayerRow()).SkillPoints);
        }

        [TestMethod]
        public async Task SpecializationMission_CompletesOnFirstPoint()
        {
            await SeedPlayerAsync();
            await CompleteAndClaimFirstMissionAsync(); // ativa a Missão 2 e dá o ponto de nível 1
            Assert.AreEqual(1, (await PlayerRow()).SkillPoints);

            var res = await Skills.DistributeAsync(7, "DoubleDrop");

            var mission = res.Missions.Single();
            Assert.AreEqual(MissionType.Specialization, mission.Type);
            Assert.AreEqual(MissionStatus.Completed, mission.Status);
        }

        [TestMethod]
        public async Task PointSpentBeforeTheMissionIsActive_StillCompletesItWhenActivated()
        {
            await SeedPlayerAsync();
            Random.Value = 0.0;
            await Mines.MineAsync(7, 1, 15); // nível 1 pela mineração, Missão 1 completa mas não resgatada
            var res = await Skills.DistributeAsync(7, "DoubleDrop");
            Assert.AreEqual(0, res.Missions.Count, "a Missão 2 ainda não foi liberada");

            var claim = await Missions.ClaimAsync(7, "FirstPickaxe");

            var second = claim.Missions.Single(m => m.Type == MissionType.Specialization);
            Assert.AreEqual(MissionStatus.Completed, second.Status, "nasce concluída: o jogador já fez o que ela pede");
        }

        private async Task SetSkillPointsAsync(int points)
        {
            await using var ctx = Db.CreateDbContext();
            (await ctx.Player.SingleAsync()).SkillPoints = points;
            await ctx.SaveChangesAsync();
        }
    }

    [TestClass]
    public class ClaimMissionTests : ShardsServiceTestBase
    {
        [TestMethod]
        public async Task NotCompleted_IsMissionNotCompleted()
        {
            await SeedPlayerAsync();

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Missions.ClaimAsync(7, "FirstPickaxe"));

            Assert.AreEqual(ShardsErrorCode.MissionNotCompleted, ex.Code);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("Nope")]
        [DataRow("1")]
        public async Task InvalidType_IsMissionNotFound(string? value)
        {
            await SeedPlayerAsync();

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Missions.ClaimAsync(7, value));

            Assert.AreEqual(ShardsErrorCode.MissionNotFound, ex.Code);
        }

        [TestMethod]
        public async Task MissionNotYetReleased_IsMissionNotFound()
        {
            await SeedPlayerAsync();

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Missions.ClaimAsync(7, "Specialization"));

            Assert.AreEqual(ShardsErrorCode.MissionNotFound, ex.Code);
        }

        [TestMethod]
        public async Task UnknownPlayer_IsPlayerNotFound()
        {
            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Missions.ClaimAsync(99, "FirstPickaxe"));

            Assert.AreEqual(ShardsErrorCode.PlayerNotFound, ex.Code);
        }

        [TestMethod]
        public async Task FirstMission_GivesXp_AndReleasesTheSecond()
        {
            await SeedPlayerAsync();
            Random.Value = 0.0;
            await Mines.MineAsync(7, 1, 10); // 10 XP de mineração, Missão 1 completa

            var res = await Missions.ClaimAsync(7, "firstpickaxe");

            Assert.AreEqual(15, res.Rewards.Xp);
            Assert.AreEqual(25, res.Player.Experience);
            Assert.AreEqual(1, res.Player.Level);
            Assert.IsTrue(res.LevelUp);
            Assert.AreEqual(1, res.Player.SkillPoints);

            Assert.AreEqual(2, res.Missions.Count);
            Assert.AreEqual(MissionStatus.Claimed, res.Missions.Single(m => m.Type == MissionType.FirstPickaxe).Status);
            Assert.AreEqual(MissionStatus.Active, res.Missions.Single(m => m.Type == MissionType.Specialization).Status);
        }

        [TestMethod]
        public async Task Claim_Twice_PaysOnlyOnce()
        {
            await SeedPlayerAsync();
            Random.Value = 0.0;
            await Mines.MineAsync(7, 1, 10);
            await Missions.ClaimAsync(7, "FirstPickaxe");
            int xpAfterFirst = (await PlayerRow()).Experience;

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Missions.ClaimAsync(7, "FirstPickaxe"));

            Assert.AreEqual(ShardsErrorCode.MissionAlreadyClaimed, ex.Code);
            Assert.AreEqual(xpAfterFirst, (await PlayerRow()).Experience);
            await using var ctx = Db.CreateDbContext();
            Assert.AreEqual(2, await ctx.PlayerMission.CountAsync(), "a missão seguinte não é criada duas vezes");
        }

        [TestMethod]
        public async Task SecondMission_GivesOres_AndReleasesTheThird()
        {
            await SeedPlayerAsync();
            await CompleteAndClaimFirstMissionAsync();
            await Skills.DistributeAsync(7, "DoubleDrop");
            int quartzBefore = await InventoryOf(OreType.Quartz);

            var res = await Missions.ClaimAsync(7, "Specialization");

            Assert.AreEqual(25, res.Rewards.Ores.Single(o => o.OreType == OreType.Quartz).Quantity);
            Assert.AreEqual(quartzBefore + 25, res.Inventory.Single(i => i.OreType == OreType.Quartz).Quantity);
            Assert.AreEqual(MissionStatus.Active, res.Missions.Single(m => m.Type == MissionType.OperationalExpansion).Status);
        }

        [TestMethod]
        public async Task ThirdMission_ReleasesTheFourth()
        {
            await SeedPlayerAsync();
            await CompleteAndClaimFirstMissionAsync();
            await Skills.DistributeAsync(7, "DoubleDrop");
            await Missions.ClaimAsync(7, "Specialization");
            await GiveOresAsync(OreType.Quartz, 200);
            await GrantMineSlotAsync();
            await Mines.UnlockNextAsync(7);

            var res = await Missions.ClaimAsync(7, "OperationalExpansion");

            Assert.AreEqual(2, res.Missions.Count, "a missão resgatada e a Primeira Gema");
            Assert.AreEqual(MissionStatus.Claimed, res.Missions.Single(m => m.Type == MissionType.OperationalExpansion).Status);
            Assert.AreEqual(MissionStatus.Active, res.Missions.Single(m => m.Type == MissionType.FirstGem).Status);
            Assert.AreEqual(MissionType.FirstGem, (await Players.GetStateAsync(7)).Missions.Single().Type);
        }

        [TestMethod]
        public async Task NextMission_StartsCompleted_WhenTheMineWasAlreadyBought()
        {
            await SeedPlayerAsync();
            await CompleteAndClaimFirstMissionAsync();
            await Skills.DistributeAsync(7, "DoubleDrop");
            await GiveOresAsync(OreType.Quartz, 200);
            await GrantMineSlotAsync();
            await Mines.UnlockNextAsync(7); // compra antes de a Missão 3 ser liberada

            var res = await Missions.ClaimAsync(7, "Specialization");

            Assert.AreEqual(MissionStatus.Completed, res.Missions.Single(m => m.Type == MissionType.OperationalExpansion).Status);
        }
    }

    [TestClass]
    public class UnlockMineTests : ShardsServiceTestBase
    {
        [TestMethod]
        public async Task WithoutResources_IsNotEnoughResources_AndChangesNothing()
        {
            await SeedPlayerAsync();
            await GrantMineSlotAsync();
            await GiveOresAsync(OreType.Quartz, 199);

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.UnlockNextAsync(uid: 7));

            Assert.AreEqual(ShardsErrorCode.NotEnoughResources, ex.Code);
            Assert.AreEqual(199, await InventoryOf(OreType.Quartz));
            await using var ctx = Db.CreateDbContext();
            Assert.AreEqual(1, await ctx.Mine.CountAsync());
        }

        [TestMethod]
        public async Task WithResources_DebitsTheCost_AndCreatesMineTwo()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 210);
            Time.Advance(TimeSpan.FromMinutes(5));

            await GrantMineSlotAsync();

            var res = await Mines.UnlockNextAsync(7);

            Assert.AreEqual(2, res.Mine.MineNumber);
            Assert.AreEqual(100, res.Mine.CartCapacity);
            Assert.AreEqual(0.33, res.Mine.OresPerMinute, 1e-9);
            Assert.AreEqual(0, res.Mine.CartAmount, "o vagonete da mina nova começa vazio");
            Assert.AreEqual(10, res.Inventory.Single().Quantity, "sobram 10 de quartzo depois de pagar 200");
            Assert.AreEqual(10, await InventoryOf(OreType.Quartz));
        }

        [TestMethod]
        public async Task SecondUnlock_WithoutAnotherSlot_IsMineSlotsFull()
        {
            await SeedPlayerAsync();
            await GrantMineSlotAsync();
            await GiveOresAsync(OreType.Quartz, 1000);
            await Mines.UnlockNextAsync(uid: 7);

            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.UnlockNextAsync(uid: 7));

            Assert.AreEqual(ShardsErrorCode.MineSlotsFull, ex.Code);
            Assert.AreEqual(800, await InventoryOf(OreType.Quartz), "só a primeira compra (200) foi debitada");
        }

        [TestMethod]
        public async Task UnknownPlayer_IsPlayerNotFound()
        {
            var ex = await Assert.ThrowsExactlyAsync<ShardsException>(() => Mines.UnlockNextAsync(99));

            Assert.AreEqual(ShardsErrorCode.PlayerNotFound, ex.Code);
        }

        [TestMethod]
        public async Task ThirdMission_CompletesOnUnlock_WhenActive()
        {
            await SeedPlayerAsync();
            await CompleteAndClaimFirstMissionAsync();
            await Skills.DistributeAsync(7, "DoubleDrop");
            await Missions.ClaimAsync(7, "Specialization");
            await GiveOresAsync(OreType.Quartz, 200);

            await GrantMineSlotAsync();

            var res = await Mines.UnlockNextAsync(7);

            var mission = res.Missions.Single();
            Assert.AreEqual(MissionType.OperationalExpansion, mission.Type);
            Assert.AreEqual(MissionStatus.Completed, mission.Status);
        }

        [TestMethod]
        public async Task NewMine_CanBeMinedAndCollectedIndependently()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 200);
            await GrantMineSlotAsync();
            await Mines.UnlockNextAsync(7);
            Time.Advance(TimeSpan.FromMinutes(10));
            Random.Value = 0.0;

            var mined = await Mines.MineAsync(7, 2, 1);
            var collected = await Mines.CollectCartAsync(7, 2);
            var cart1 = (await Players.GetStateAsync(7)).Mines.Single(m => m.MineNumber == 1);

            Assert.AreEqual(1, mined.Drops.Single().Quantity);
            Assert.AreEqual(3, collected.Total);
            Assert.AreEqual(3, cart1.CartAmount, "a Mina 1 continua acumulando por conta própria");
        }

        [TestMethod]
        public async Task StateAfterUnlock_OffersMineThree_WithoutSlot()
        {
            await SeedPlayerAsync();
            await GiveOresAsync(OreType.Quartz, 200);

            await GrantMineSlotAsync();

            await Mines.UnlockNextAsync(7);

            var state = await Players.GetStateAsync(7);
            Assert.AreEqual(3, state.NextMine!.MineNumber);
            Assert.AreEqual(600, state.NextMine.Cost.Single().Quantity);
            Assert.IsFalse(state.NextMine.HasSlot, "o único espaço já foi usado pela Mina 2");
            Assert.AreEqual(2, state.Mines.Count);
        }
    }

    [TestClass]
    public class OnboardingFlowTests : ShardsServiceTestBase
    {
        [TestMethod]
        public async Task FullOnboarding_FromFirstAccessToSecondMine()
        {
            // 1) primeiro acesso: Missão 1 ativa
            var state = await Players.GetStateAsync(7);
            Assert.AreEqual(MissionType.FirstPickaxe, state.Missions.Single().Type);

            // 2) minera 10 vezes e resgata: nível 1, ponto de habilidade e Missão 2
            Random.Value = 0.0;
            await Mines.MineAsync(7, 1, 10);
            await Missions.ClaimAsync(7, "FirstPickaxe");
            state = await Players.GetStateAsync(7);
            Assert.AreEqual(1, state.Player.Level);
            Assert.AreEqual(1, state.Player.SkillPoints);
            Assert.AreEqual(MissionType.Specialization, state.Missions.Single().Type);

            // 3) distribui o ponto e resgata: 25 quartzo e Missão 3
            await Skills.DistributeAsync(7, "DoubleDrop");
            await Missions.ClaimAsync(7, "Specialization");
            state = await Players.GetStateAsync(7);
            Assert.AreEqual(MissionType.OperationalExpansion, state.Missions.Single().Type);
            Assert.AreEqual(MissionStatus.Active, state.Missions.Single().Status);
            Assert.IsFalse(state.NextMine!.CanAfford);

            // 4) junta os recursos (mineração + vagonete ao longo do tempo), compra a Mina 2 e resgata a Missão 3
            await GiveOresAsync(OreType.Quartz, 200);
            state = await Players.GetStateAsync(7);
            Assert.IsTrue(state.NextMine!.CanAfford);

            await GrantMineSlotAsync();

            await Mines.UnlockNextAsync(7);
            await Missions.ClaimAsync(7, "OperationalExpansion");

            state = await Players.GetStateAsync(7);
            Assert.AreEqual(2, state.Mines.Count);
            Assert.AreEqual(3, state.NextMine!.MineNumber, "a Mina 3 é a próxima, mas sem espaço livre");
            Assert.AreEqual(MissionType.FirstGem, state.Missions.Single().Type, "depois da Mina 2, o próximo objetivo é a primeira gema");
        }
    }
}
