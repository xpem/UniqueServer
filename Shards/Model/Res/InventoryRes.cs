using Shards.Model.DTO;

namespace Shards.Model.Res
{
    public record InventoryItemRes
    {
        public OreType OreType { get; set; }

        public ItemStage Stage { get; set; }

        public int Quantity { get; set; }
    }

    /// <summary>Inventário completo do jogador, por minério e estágio (só quantidade maior que zero).</summary>
    public record InventoryRes
    {
        public List<InventoryItemRes> Items { get; set; } = [];
    }
}
