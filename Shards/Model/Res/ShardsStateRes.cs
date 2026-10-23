using Shards.Model.DTO;

namespace Shards.Model.Res
{
    public record OreAmountRes
    {
        public OreType OreType { get; set; }

        public int Quantity { get; set; }
    }

    public record PlayerRes
    {
        public int Level { get; set; }

        public int Experience { get; set; }

        public int ExperienceToNextLevel { get; set; }

        public int SkillPoints { get; set; }

        public int Energy { get; set; }

        public int MaxEnergy { get; set; }

        /// <summary>Segundos para regenerar 1 ponto de energia (vem da configuração; o cliente usa para a barra ao vivo).</summary>
        public int SecondsPerEnergy { get; set; }

        /// <summary>Quando o próximo ponto de energia chega; nulo com a energia cheia.</summary>
        public DateTime? NextEnergyAtUtc { get; set; }
    }

    public record SkillRes
    {
        public SkillType Type { get; set; }

        public int Level { get; set; }

        public int MaxLevel { get; set; }
    }

    public record MineStateRes
    {
        public int MineNumber { get; set; }

        /// <summary>Minério em que a mina se especializa (Quartz por padrão).</summary>
        public OreType Production { get; set; }

        public int CartCapacity { get; set; }

        /// <summary>Nível da melhoria do vagonete (0 = capacidade inicial).</summary>
        public int CartLevel { get; set; }

        public int CartAmount { get; set; }

        /// <summary>Quando o vagonete enche; nulo se a mina não gera minérios.</summary>
        public DateTime? CartFullAtUtc { get; set; }

        public double OresPerMinute { get; set; }
    }

    public record MissionRewardRes
    {
        public int Xp { get; set; }

        public List<OreAmountRes> Ores { get; set; } = [];
    }

    public record MissionRes
    {
        public MissionType Type { get; set; }

        public required string Name { get; set; }

        public required string Description { get; set; }

        public MissionStatus Status { get; set; }

        public int Progress { get; set; }

        public int Goal { get; set; }

        public required MissionRewardRes Reward { get; set; }
    }

    public record NextMineRes
    {
        public int MineNumber { get; set; }

        public List<OreAmountRes> Cost { get; set; } = [];

        /// <summary>O inventário atual cobre o custo.</summary>
        public bool CanAfford { get; set; }

        /// <summary>Há espaço livre para mais uma mina (skill MineSlots). Sem espaço, comprar dá MineSlotsFull.</summary>
        public bool HasSlot { get; set; }
    }

    /// <summary>Espaços de mina: quantas o jogador tem e quantas pode ter.</summary>
    public record MineSlotsRes
    {
        public int Used { get; set; }

        public int Total { get; set; }
    }

    /// <summary>A próxima melhoria do vagonete de uma mina.</summary>
    public record NextCartUpgradeRes
    {
        public int Level { get; set; }

        /// <summary>Capacidade do vagonete depois da melhoria.</summary>
        public int Capacity { get; set; }

        public List<OreAmountRes> Cost { get; set; } = [];

        /// <summary>O inventário atual cobre o custo (sem contar o que o vagonete entregaria na melhoria).</summary>
        public bool CanAfford { get; set; }

        /// <summary>A skill CartCapacity já está num nível que permite esta melhoria. Se não, subir dá CartUpgradeLocked.</summary>
        public bool SkillAllows { get; set; }
    }

    /// <summary>O estado da melhoria do vagonete de cada mina. Next é nulo quando a mina já está no nível máximo.</summary>
    public record MineCartUpgradeRes
    {
        public int MineNumber { get; set; }

        public int CartLevel { get; set; }

        public NextCartUpgradeRes? Next { get; set; }
    }

    public record DropRes
    {
        public OreType OreType { get; set; }

        /// <summary>Probabilidade de 0 a 1, já normalizada pela soma da tabela.</summary>
        public double Chance { get; set; }

        public int Xp { get; set; }
    }

    /// <summary>Um tipo de produção que o jogador pode escolher para uma mina.</summary>
    public record ProductionOptionRes
    {
        public OreType Focus { get; set; }

        /// <summary>Custo, por mina, para passar a produzir este tipo. Vazio quando é de graça (Quartz).</summary>
        public List<OreAmountRes> SwitchCost { get; set; } = [];

        public List<DropRes> Drops { get; set; } = [];

        /// <summary>O inventário atual cobre o custo (sem contar o que o vagonete entregaria na troca).</summary>
        public bool CanAfford { get; set; }
    }

    /// <summary>Por gema, o que o jogador tem do estágio consumido e se consegue fazer ao menos 1 lapidação agora.</summary>
    public record WorkshopGemRes
    {
        public OreType OreType { get; set; }

        /// <summary>Quantas gemas do estágio consumido o jogador tem (por exemplo, brutas para a receita Polished).</summary>
        public int Available { get; set; }

        /// <summary>O inventário cobre as gemas e o quartzo de 1 lapidação. Não considera a skill (ver SkillAllows).</summary>
        public bool CanAfford { get; set; }
    }

    /// <summary>Uma receita da oficina: produz 1 gema do estágio Stage a partir de InputQuantity gemas do estágio anterior.</summary>
    public record WorkshopRecipeRes
    {
        /// <summary>Estágio produzido (Polished ou Faceted).</summary>
        public ItemStage Stage { get; set; }

        /// <summary>Nível da skill Workshop que libera a receita.</summary>
        public int Level { get; set; }

        /// <summary>Gemas do estágio anterior consumidas por gema produzida.</summary>
        public int InputQuantity { get; set; }

        /// <summary>Minério bruto consumido por gema produzida.</summary>
        public List<OreAmountRes> Cost { get; set; } = [];

        /// <summary>A skill Workshop já permite a receita. Se não, craft dá WorkshopLocked.</summary>
        public bool SkillAllows { get; set; }

        public List<WorkshopGemRes> Gems { get; set; } = [];
    }

    /// <summary>Resposta de GET /shards/state: o estado completo do jogador.</summary>
    public record ShardsStateRes
    {
        public DateTime ServerTimeUtc { get; set; }

        public required PlayerRes Player { get; set; }

        public List<SkillRes> Skills { get; set; } = [];

        public List<OreAmountRes> Inventory { get; set; } = [];

        public List<MineStateRes> Mines { get; set; } = [];

        /// <summary>Missões visíveis: ativas, ou concluídas ainda não resgatadas.</summary>
        public List<MissionRes> Missions { get; set; } = [];

        /// <summary>Próxima mina à venda; nulo se não houver.</summary>
        public NextMineRes? NextMine { get; set; }

        public required MineSlotsRes MineSlots { get; set; }

        /// <summary>Produções que podem ser escolhidas por mina, com custo e probabilidades.</summary>
        public List<ProductionOptionRes> Productions { get; set; } = [];

        /// <summary>Melhoria do vagonete de cada mina: nível atual e próxima melhoria (custo, skill e saldo).</summary>
        public List<MineCartUpgradeRes> CartUpgrades { get; set; } = [];

        /// <summary>Receitas da oficina, com custo, skill e saldo por gema.</summary>
        public List<WorkshopRecipeRes> Workshop { get; set; } = [];

        /// <summary>Missões diárias; nulo até o jogador resgatar a Missão 1.</summary>
        public DailyStateRes? Daily { get; set; }
    }
}
