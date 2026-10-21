using System.ComponentModel.DataAnnotations.Schema;

namespace Shards.Model.DTO
{
    [Table("Player")]
    public class PlayerDTO
    {
        public int Id { get; set; }

        public required int UserId { get; set; }

        public required DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int Level { get; set; }

        public int Experience { get; set; }

        public int SkillPoints { get; set; }

        public int Energy { get; set; } = 100;

        public int MaxEnergy { get; set; } = 100;

        public required DateTime LastEnergyUpdateUtc { get; set; }

        /// <summary>Dias seguidos com o baú diário resgatado (vale enquanto LastChestDay for hoje ou ontem).</summary>
        public int DailyStreak { get; set; }

        /// <summary>Último dia em que o baú diário foi resgatado.</summary>
        public DateOnly? LastChestDay { get; set; }

        public uint Version { get; set; }
    }
}
