using System.ComponentModel.DataAnnotations.Schema;

namespace Shards.Model.DTO
{
    [Table("InventoryItem")]
    public class InventoryItemDTO
    {
        public int Id { get; set; }

        public required int PlayerId { get; set; }

        public required OreType OreType { get; set; }

        /// <summary>Estágio do item (bruto, polido, facetado). Uma linha por (jogador, minério, estágio).</summary>
        public ItemStage Stage { get; set; } = ItemStage.Raw;

        public int Quantity { get; set; }

        public uint Version { get; set; }
    }
}
