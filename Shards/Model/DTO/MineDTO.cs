using System.ComponentModel.DataAnnotations.Schema;

namespace Shards.Model.DTO
{
    [Table("Mine")]
    public class MineDTO
    {
        public int Id { get; set; }

        public required int PlayerId { get; set; }

        /// <summary>1 = mina inicial, 2 = segunda mina...</summary>
        public required int MineNumber { get; set; }

        public required DateTime CreatedAt { get; set; }

        public int CartCapacity { get; set; } = 100;

        public required DateTime LastCartCollectionUtc { get; set; }

        /// <summary>Auto-mineração: 0.33 = ~1 minério a cada 3 minutos.</summary>
        public double OresPerMinute { get; set; } = 0.33;

        public uint Version { get; set; }
    }
}
