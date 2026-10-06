namespace Shards.Model.Res
{
    /// <summary>Resposta de POST /shards/mines/{mineNumber}/mine.</summary>
    public record MineRes
    {
        public List<OreAmountRes> Drops { get; set; } = [];

        /// <summary>Quantas das mineirações renderam drop duplo (skill DoubleDrop).</summary>
        public int DoubleDrops { get; set; }

        public int XpGained { get; set; }

        public bool LevelUp { get; set; }

        public required PlayerRes Player { get; set; }

        /// <summary>Missões que mudaram nesta ação.</summary>
        public List<MissionRes> Missions { get; set; } = [];
    }

    /// <summary>Resposta de POST /shards/mines/{mineNumber}/collect-cart.</summary>
    public record CollectCartRes
    {
        public List<OreAmountRes> Collected { get; set; } = [];

        public int Total { get; set; }

        public required MineStateRes Cart { get; set; }

        public List<OreAmountRes> Inventory { get; set; } = [];
    }

    /// <summary>Resposta de POST /shards/skills/distribute.</summary>
    public record DistributeSkillRes
    {
        public List<SkillRes> Skills { get; set; } = [];

        public int SkillPoints { get; set; }

        public List<MissionRes> Missions { get; set; } = [];
    }

    /// <summary>Resposta de POST /shards/mines/unlock.</summary>
    public record UnlockMineRes
    {
        public required MineStateRes Mine { get; set; }

        public List<OreAmountRes> Inventory { get; set; } = [];

        public List<MissionRes> Missions { get; set; } = [];
    }

    /// <summary>Resposta de POST /shards/missions/{missionType}/claim.</summary>
    public record ClaimMissionRes
    {
        public required MissionRewardRes Rewards { get; set; }

        public bool LevelUp { get; set; }

        public required PlayerRes Player { get; set; }

        public List<OreAmountRes> Inventory { get; set; } = [];

        /// <summary>A missão resgatada e a nova missão ativada.</summary>
        public List<MissionRes> Missions { get; set; } = [];
    }
}
