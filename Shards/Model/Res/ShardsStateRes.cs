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

        public int CartCapacity { get; set; }

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

        public bool CanAfford { get; set; }
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
    }
}
