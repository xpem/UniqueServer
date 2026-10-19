namespace Shards.Model.Res
{
    /// <summary>Resposta de POST /shards/workshop/craft.</summary>
    public record CraftRes
    {
        /// <summary>O que foi produzido.</summary>
        public required InventoryItemRes Crafted { get; set; }

        /// <summary>O que foi gasto: as gemas do estágio anterior e o custo em minério bruto.</summary>
        public List<InventoryItemRes> Spent { get; set; } = [];

        /// <summary>Inventário completo depois da lapidação.</summary>
        public required InventoryRes Inventory { get; set; }

        /// <summary>Missões que mudaram nesta ação.</summary>
        public List<MissionRes> Missions { get; set; } = [];
    }
}
