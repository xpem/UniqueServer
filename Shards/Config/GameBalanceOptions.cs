using Shards.Model.DTO;

namespace Shards.Config
{
    /// <summary>
    /// Balanceamento do jogo, lido da seção "Shards:Balance" do appsettings.
    /// As listas começam vazias de propósito: o binder do .NET acrescenta itens à lista existente,
    /// então os valores reais ficam só no appsettings.json (fonte única).
    /// </summary>
    public class GameBalanceOptions
    {
        public const string SectionName = "Shards:Balance";

        public EnergyOptions Energy { get; set; } = new();

        public ExperienceOptions Experience { get; set; } = new();

        public DoubleDropOptions DoubleDrop { get; set; } = new();

        public List<MineOptions> Mines { get; set; } = [];

        /// <summary>Tipos de produção que uma mina pode ter (Quartz é o padrão de toda mina).</summary>
        public List<ProductionOptions> Productions { get; set; } = [];
    }

    public class EnergyOptions
    {
        public int MaxEnergy { get; set; } = 100;

        public int CostPerMine { get; set; } = 1;

        /// <summary>Máximo de mineirações aceitas numa só chamada (campo times).</summary>
        public int MaxMinesPerRequest { get; set; } = 100;

        /// <summary>Segundos para regenerar 1 ponto (216 s = 100 pontos em 6 h).</summary>
        public int SecondsPerPoint { get; set; } = 216;
    }

    public class ExperienceOptions
    {
        /// <summary>XP acumulado para o nível N = XpFirstLevel * N * (N + 1) / 2 (15, 45, 90, 150, 225...).</summary>
        public int XpFirstLevel { get; set; } = 15;
    }

    public class DoubleDropOptions
    {
        public int MaxLevel { get; set; } = 5;

        public double ChancePerLevel { get; set; } = 0.15;
    }

    public class MineOptions
    {
        public int MineNumber { get; set; }

        public int CartCapacity { get; set; } = 100;

        public double OresPerMinute { get; set; } = 0.33;

        /// <summary>Custo para desbloquear esta mina. Vazio na mina inicial.</summary>
        public List<OreAmount> UnlockCost { get; set; } = [];
    }

    /// <summary>
    /// Um tipo de produção de mina: o que ela sorteia a cada mineração e a cada minério do vagonete, e quanto custa
    /// pôr uma mina nele. Voltar ao Quartz é de graça (SwitchCost vazio).
    /// </summary>
    public class ProductionOptions
    {
        /// <summary>Minério em que a mina se especializa.</summary>
        public OreType Focus { get; set; }

        /// <summary>Custo, por mina e a cada mudança, para passar a produzir este tipo.</summary>
        public List<OreAmount> SwitchCost { get; set; } = [];

        public List<DropOptions> Drops { get; set; } = [];
    }

    public class DropOptions
    {
        public OreType Ore { get; set; }

        /// <summary>Peso relativo do sorteio (0.85 e 0.15 somam 1, mas a soma é normalizada).</summary>
        public double Chance { get; set; }

        public int Xp { get; set; }
    }
}
