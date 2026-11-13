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

        public MineSlotsOptions MineSlots { get; set; } = new();

        /// <summary>Melhorias do vagonete (níveis 1..N). O nível máximo da skill CartCapacity é o maior nível daqui.</summary>
        public List<CartUpgradeOptions> CartUpgrades { get; set; } = [];

        /// <summary>Receitas da oficina (uma por estágio de destino). O nível máximo da skill Workshop é o maior nível daqui.</summary>
        public List<WorkshopStepOptions> Workshop { get; set; } = [];

        public DailyOptions Daily { get; set; } = new();

        public List<MineOptions> Mines { get; set; } = [];

        /// <summary>Tipos de produção que uma mina pode ter (Quartz é o padrão de toda mina).</summary>
        public List<ProductionOptions> Productions { get; set; } = [];

        /// <summary>Catálogo de minérios: tipo, elemento, se pode ser lapidado e o ataque por estágio. Todo OreType precisa ter uma entrada.</summary>
        public List<OreOptions> Ores { get; set; } = [];

        /// <summary>Regras de combate: o ciclo de elementos e os multiplicadores de vantagem e desvantagem.</summary>
        public CombatOptions Combat { get; set; } = new();

        /// <summary>Energia de exploração: a segunda barra do jogador, só para extrair dos depósitos.</summary>
        public ExploreOptions Explore { get; set; } = new();

        /// <summary>Depósitos (minas finitas): oferta semanal, faixas de dificuldade e rotação.</summary>
        public DepositsOptions Deposits { get; set; } = new();
    }

    /// <summary>Energia de exploração. Mesma regra da energia normal (regenera sem perder o resto parcial), com máximo e ritmo próprios.</summary>
    public class ExploreOptions
    {
        public int MaxEnergy { get; set; } = 72;

        /// <summary>Segundos para regenerar 1 ponto (1.200 s = 20 minutos, cerca de 10 h para encher 30).</summary>
        public int SecondsPerPoint { get; set; } = 1200;
    }

    /// <summary>
    /// Depósitos. A semana começa na segunda-feira, na hora de virada das diárias (Daily.ResetHourUtc). Cada semana o jogador recebe
    /// uma oferta por faixa (Tiers). Um depósito intocado expira no fim da semana; um já atacado persiste até se esgotar.
    /// </summary>
    public class DepositsOptions
    {
        /// <summary>Nível mínimo da skill Workshop para receber depósitos.</summary>
        public int UnlockWorkshopLevel { get; set; } = 1;

        /// <summary>Máximo de depósitos já atacados (abertos) ao mesmo tempo. Quem recusa é o ataque; aqui só o limite.</summary>
        public int MaxOpen { get; set; } = 3;

        /// <summary>Estoque inicial = defesa sorteada x este valor (mina mais difícil, mais rica).</summary>
        public double StockPerDefense { get; set; } = 2.5;

        /// <summary>Máximo de pilhas distintas (minério + estágio) por lista (atacantes e defensores) numa chamada de ataque.</summary>
        public int MaxAttackStacks { get; set; } = 10;

        /// <summary>Energia de exploração gasta por unidade extraída (cada unidade sorteia a tabela de drop da faixa).</summary>
        public int EnergyPerExtraction { get; set; } = 1;

        public DepositRerollOptions Reroll { get; set; } = new();

        /// <summary>Uma faixa por slot, de 1 a N (leve, média, difícil).</summary>
        public List<DepositTierOptions> Tiers { get; set; } = [];
    }

    /// <summary>
    /// Troca de uma oferta intocada. As primeiras FreePerWeek trocas da semana (somando todos os slots) são grátis;
    /// a k-ésima paga custa Cost x CostMultiplier^(k-1).
    /// </summary>
    public class DepositRerollOptions
    {
        public int FreePerWeek { get; set; } = 1;

        public OreAmount Cost { get; set; } = new() { Ore = OreType.Quartz, Quantity = 500 };

        public int CostMultiplier { get; set; } = 2;
    }

    /// <summary>Uma faixa de dificuldade: defesa (sorteada em Defense +- DefenseVariance) e a tabela do que o depósito rende.</summary>
    public class DepositTierOptions
    {
        public int Slot { get; set; }

        public string Name { get; set; } = "";

        public int Defense { get; set; }

        /// <summary>Fração da defesa para mais e para menos (0,2 = de 80% a 120%).</summary>
        public double DefenseVariance { get; set; }

        /// <summary>
        /// Contra-ataque do depósito por ponto de dano causado (0,05 = 5%). O que as gemas de defesa não absorvem
        /// tira energia de exploração (Combat.EnergyPerCounterPoint). Cresce nas faixas mais difíceis.
        /// </summary>
        public double CounterRatio { get; set; }

        /// <summary>Tabela de drop da extração (pesos normalizados pela soma). Quartzo e moeda não entram.</summary>
        public List<DropOptions> Drops { get; set; } = [];
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

    /// <summary>
    /// Skill que libera espaços para ter mais minas: espaços = BaseSlots + nível. Com BaseSlots 1 e MaxLevel 4, o limite é de 5 minas.
    /// Cada mina nova também custa o UnlockCost dela (espaço e quartzo juntos).
    /// </summary>
    public class MineSlotsOptions
    {
        public int BaseSlots { get; set; } = 1;

        public int MaxLevel { get; set; } = 4;
    }

    /// <summary>
    /// Uma melhoria do vagonete de uma mina: o nível, a capacidade que ele passa a ter e o custo (por mina).
    /// A skill CartCapacity é o teto: com a skill no nível N, qualquer mina pode ser ampliada até o nível N.
    /// </summary>
    public class CartUpgradeOptions
    {
        public int Level { get; set; }

        public int Capacity { get; set; }

        public List<OreAmount> Cost { get; set; } = [];
    }

    /// <summary>
    /// Uma receita da oficina: gasta InputQuantity gemas do estágio anterior (a mesma gema) mais Cost em minério bruto
    /// e produz 1 gema do estágio Stage. Level é o nível da skill Workshop que libera a receita (Polished = 1, Faceted = 2).
    /// </summary>
    public class WorkshopStepOptions
    {
        public int Level { get; set; }

        /// <summary>Estágio produzido (Polished ou Faceted). O estágio consumido é o anterior.</summary>
        public ItemStage Stage { get; set; }

        public int InputQuantity { get; set; }

        public List<OreAmount> Cost { get; set; } = [];
    }

    /// <summary>
    /// Missões diárias. O dia vira em ResetHourUtc (3 = meia-noite em Brasília). Cada dia o jogador recebe MissionsPerDay tarefas
    /// (Login mais sorteadas) e, ao resgatar todas, pode resgatar o baú; dias seguidos de baú formam a sequência (streak).
    /// </summary>
    public class DailyOptions
    {
        public int ResetHourUtc { get; set; } = 3;

        public int MissionsPerDay { get; set; } = 3;

        public DailyChestOptions Chest { get; set; } = new();

        public DailyStreakOptions Streak { get; set; } = new();
    }

    public class DailyChestOptions
    {
        public int Xp { get; set; } = 10;

        public int Quartz { get; set; } = 100;
    }

    /// <summary>
    /// Bônus da sequência: o baú do dia N do ciclo paga Quartz + (N - 1) * QuartzPerDay; no último dia do ciclo o total é multiplicado por
    /// CycleBonusMultiplier. Depois do ciclo recomeça. Faltar um dia zera a sequência.
    /// </summary>
    public class DailyStreakOptions
    {
        public int CycleDays { get; set; } = 7;

        public int QuartzPerDay { get; set; } = 20;

        public int CycleBonusMultiplier { get; set; } = 2;
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

    /// <summary>
    /// Uma entrada do catálogo de minérios. Um minério novo é uma linha aqui mais o valor no enum OreType (sem lista fixa no código).
    /// </summary>
    public class OreOptions
    {
        public OreType Ore { get; set; }

        public OreKind Kind { get; set; }

        /// <summary>Elemento do minério (Neutral para a moeda e para exclusivos neutros).</summary>
        public Element Element { get; set; } = Element.Neutral;

        /// <summary>A oficina aceita lapidar este minério. A ametista só será craftable quando for possível obtê-la.</summary>
        public bool Craftable { get; set; }

        /// <summary>Ataque por estágio (Polished e Faceted). Gema bruta (Raw) não ataca.</summary>
        public Dictionary<ItemStage, int> Attack { get; set; } = new();

        /// <summary>Defesa por estágio (Polished e Faceted): quanto a gema absorve do contra-ataque. Gema bruta (Raw) não defende.</summary>
        public Dictionary<ItemStage, int> Defense { get; set; } = new();
    }

    /// <summary>
    /// Combate. Effectiveness é a matriz de efetividade (atacante x defensor): o multiplicador que o ataque de um elemento recebe contra
    /// a defesa de outro (1,5 = vantagem; 0,75 = desvantagem; 0 = imune). Par que não aparece na matriz vale 1.
    /// </summary>
    public class CombatOptions
    {
        public List<EffectivenessOptions> Effectiveness { get; set; } = [];

        /// <summary>
        /// Pontos de energia de exploração perdidos por ponto de contra-ataque que as gemas de defesa não absorveram
        /// (a conta fecha com o custo de defender: ver SIMULACAO-ECONOMIA.md).
        /// </summary>
        public double EnergyPerCounterPoint { get; set; } = 3;
    }

    public class EffectivenessOptions
    {
        public Element Attacker { get; set; }

        public Element Defender { get; set; }

        public double Multiplier { get; set; } = 1;
    }

    public class DropOptions
    {
        public OreType Ore { get; set; }

        /// <summary>Peso relativo do sorteio (0.85 e 0.15 somam 1, mas a soma é normalizada).</summary>
        public double Chance { get; set; }

        public int Xp { get; set; }
    }
}
