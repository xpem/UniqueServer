using Shards.Model.DTO;

namespace Shards.Model.Res
{
    public record DailyMissionRes
    {
        public DailyMissionType Type { get; set; }

        public required string Name { get; set; }

        public required string Description { get; set; }

        public MissionStatus Status { get; set; }

        public int Progress { get; set; }

        public int Goal { get; set; }

        public required MissionRewardRes Reward { get; set; }
    }

    public record DailyChestRes
    {
        /// <summary>O baú de hoje já foi resgatado.</summary>
        public bool Claimed { get; set; }

        /// <summary>Todas as diárias de hoje foram resgatadas e o baú ainda não.</summary>
        public bool CanClaim { get; set; }

        /// <summary>O que o próximo baú paga (o de hoje, ou o de amanhã se o de hoje já foi resgatado).</summary>
        public required MissionRewardRes Reward { get; set; }
    }

    public record DailyStreakRes
    {
        /// <summary>Dias seguidos com o baú resgatado (0 se a sequência quebrou).</summary>
        public int Days { get; set; }

        /// <summary>Dias do ciclo da sequência (o último dia do ciclo paga o bônus).</summary>
        public int CycleDays { get; set; }
    }

    /// <summary>As missões diárias do jogador. Nulo no state enquanto a Missão 1 não foi resgatada.</summary>
    public record DailyStateRes
    {
        /// <summary>Dia do jogo (yyyy-MM-dd). Vira no horário de reset, não à meia-noite UTC.</summary>
        public required string Day { get; set; }

        /// <summary>Quando o dia vira e as diárias são trocadas. Depois disso, refaça o GET /state.</summary>
        public DateTime ResetsAtUtc { get; set; }

        public List<DailyMissionRes> Missions { get; set; } = [];

        public required DailyChestRes Chest { get; set; }

        public required DailyStreakRes Streak { get; set; }
    }

    /// <summary>Resposta de POST /shards/daily/{type}/claim e POST /shards/daily/chest/claim.</summary>
    public record DailyClaimRes
    {
        public required MissionRewardRes Rewards { get; set; }

        public bool LevelUp { get; set; }

        public required PlayerRes Player { get; set; }

        public List<OreAmountRes> Inventory { get; set; } = [];

        public required DailyStateRes Daily { get; set; }
    }
}
