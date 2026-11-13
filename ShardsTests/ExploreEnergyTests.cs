using Microsoft.EntityFrameworkCore;

namespace ShardsTests
{
    [TestClass]
    public class ExploreEnergyTests : ShardsServiceTestBase
    {
        private async Task SetExploreAsync(int energy, DateTime last, int uid = 7)
        {
            await using var ctx = Db.CreateDbContext();
            var player = await ctx.Player.SingleAsync(p => p.UserId == uid);
            player.ExploreEnergy = energy;
            player.LastExploreEnergyUpdateUtc = last;
            await ctx.SaveChangesAsync();
        }

        [TestMethod]
        public async Task NewPlayer_StartsWithAFullExploreBar()
        {
            var player = (await Players.GetStateAsync(7)).Player;

            Assert.AreEqual(Balance.Explore.MaxEnergy, player.ExploreEnergy);
            Assert.AreEqual(Balance.Explore.MaxEnergy, player.ExploreMaxEnergy);
            Assert.AreEqual(1200, player.ExploreSecondsPerEnergy);
            Assert.IsNull(player.NextExploreEnergyAtUtc);
        }

        [TestMethod]
        public async Task ExploreBar_RegeneratesOnItsOwnPace_KeepingThePartialRemainder()
        {
            await SeedPlayerAsync();
            await SetExploreAsync(10, Start);

            Time.Advance(TimeSpan.FromSeconds(3 * 1200 + 600));
            var player = (await Players.GetStateAsync(7)).Player;

            Assert.AreEqual(13, player.ExploreEnergy, "3 pontos a cada 1.200 s; os 600 s restantes não contam ainda");
            Assert.AreEqual(Start.AddSeconds(4 * 1200), player.NextExploreEnergyAtUtc, "o resto parcial foi preservado");
        }

        [TestMethod]
        public async Task ExploreBar_IsIndependentFromTheNormalEnergy()
        {
            await SeedPlayerAsync(energy: 40, lastEnergyUpdate: Start);
            await SetExploreAsync(10, Start);

            Time.Advance(TimeSpan.FromSeconds(1200));
            var player = (await Players.GetStateAsync(7)).Player;

            Assert.AreEqual(11, player.ExploreEnergy);
            Assert.AreEqual(40 + 1200 / 216, player.Energy);
        }

        [TestMethod]
        public async Task ExploreBar_CapsAtTheMaximum()
        {
            await SeedPlayerAsync();
            await SetExploreAsync(25, Start);

            Time.Advance(TimeSpan.FromDays(2));
            var player = (await Players.GetStateAsync(7)).Player;

            Assert.AreEqual(Balance.Explore.MaxEnergy, player.ExploreEnergy);
            Assert.IsNull(player.NextExploreEnergyAtUtc);
        }

        [TestMethod]
        public async Task ActionResponses_AlsoCarryTheRegeneratedExploreBar()
        {
            await SeedPlayerAsync();
            await SetExploreAsync(10, Start);

            Time.Advance(TimeSpan.FromSeconds(2 * 1200));
            Random.Value = 0.0;
            var res = await Mines.MineAsync(7, 1, 1);

            Assert.AreEqual(12, res.Player.ExploreEnergy);
        }
    }
}
