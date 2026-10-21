using System.ComponentModel.DataAnnotations.Schema;

namespace Shards.Model.DTO
{
    /// <summary>Uma missão diária de um jogador num dia. Única por (jogador, dia, tipo); o dia vira no horário de reset configurado.</summary>
    [Table("DailyMission")]
    public class DailyMissionDTO
    {
        public int Id { get; set; }

        public required int PlayerId { get; set; }

        /// <summary>Dia do jogo (a virada é em Daily.ResetHourUtc, não à meia-noite UTC).</summary>
        public required DateOnly Day { get; set; }

        public required DailyMissionType DailyType { get; set; }

        public MissionStatus Status { get; set; } = MissionStatus.Active;

        public int Progress { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? ClaimedAt { get; set; }

        public uint Version { get; set; }
    }
}
