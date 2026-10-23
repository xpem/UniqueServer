using System.ComponentModel.DataAnnotations.Schema;

namespace Shards.Model.DTO
{
    [Table("PlayerMission")]
    public class PlayerMissionDTO
    {
        public int Id { get; set; }

        public required int PlayerId { get; set; }

        public required MissionType MissionType { get; set; }

        public MissionStatus Status { get; set; } = MissionStatus.Active;

        public int Progress { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? ClaimedAt { get; set; }

        public uint Version { get; set; }
    }
}
