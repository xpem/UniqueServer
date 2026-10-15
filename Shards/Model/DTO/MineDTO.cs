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

        /// <summary>Nível da melhoria do vagonete (0 = capacidade inicial). Sobe com POST upgrade-cart; limitado pela skill CartCapacity.</summary>
        public int CartLevel { get; set; }

        public required DateTime LastCartCollectionUtc { get; set; }

        /// <summary>Auto-mineração: 0.33 = ~1 minério a cada 3 minutos.</summary>
        public double OresPerMinute { get; set; } = 0.33;

        /// <summary>Minério em que a mina se especializa. Quartzo por padrão; mudar custa minério (ver Productions).</summary>
        public OreType ProductionType { get; set; } = OreType.Quartz;

        public uint Version { get; set; }
    }
}
