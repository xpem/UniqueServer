using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Shards.Config;
using Shards.Rules;

namespace ShardsTests
{
    /// <summary>Relógio controlável para os testes.</summary>
    public class FakeTimeProvider(DateTimeOffset start) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = start;

        public override DateTimeOffset GetUtcNow() => Now;

        public void Advance(TimeSpan span) => Now += span;
    }

    /// <summary>Random que sempre devolve o mesmo valor, para sorteios determinísticos.</summary>
    public class FixedRandom(double value) : Random
    {
        public double Value { get; set; } = value;

        public override double NextDouble() => Value;
    }

    public abstract class GameRulesTestBase
    {
        protected static readonly DateTime Start = new(2026, 10, 6, 12, 0, 0, DateTimeKind.Utc);

        protected FakeTimeProvider Time { get; } = new(new DateTimeOffset(Start));

        protected FixedRandom Random { get; } = new(0);

        protected GameBalanceOptions Balance { get; } = LoadBalance();

        protected GameRules Rules { get; }

        protected GameRulesTestBase()
        {
            Rules = new GameRules(Options.Create(Balance), Time, Random);
        }

        // carrega o appsettings.json real do host (copiado para a saída do projeto de testes)
        public static GameBalanceOptions LoadBalance()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            return config.GetSection(GameBalanceOptions.SectionName).Get<GameBalanceOptions>()
                ?? throw new InvalidOperationException("Seção Shards:Balance ausente no appsettings.json.");
        }
    }
}
