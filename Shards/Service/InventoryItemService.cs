using Shards.Errors;
using Shards.Model.DTO;
using Shards.Repo;

namespace Shards.Service
{
    /// <summary>
    /// Operações sobre o inventário do jogador, sobre entidades já carregadas e rastreadas pelo DbContext do comando
    /// (quem chama faz o SaveChanges junto com o resto do comando).
    /// Soma e saldo recebem o estágio (padrão Raw). Custos do jogo são sempre em minério bruto.
    /// </summary>
    public static class InventoryItemService
    {
        /// <summary>Soma ao inventário, criando a linha do (minério, estágio) se ainda não existir.</summary>
        public static void Add(ShardsDbctx ctx, List<InventoryItemDTO> inventory, int playerId, OreType ore, int quantity, ItemStage stage = ItemStage.Raw)
        {
            if (quantity <= 0) return;

            InventoryItemDTO? item = inventory.FirstOrDefault(i => i.OreType == ore && i.Stage == stage);
            if (item is null)
            {
                item = new InventoryItemDTO { PlayerId = playerId, OreType = ore, Stage = stage };
                inventory.Add(item);
                ctx.InventoryItem.Add(item);
            }

            item.Quantity += quantity;
        }

        public static int QuantityOf(IEnumerable<InventoryItemDTO> inventory, OreType ore, ItemStage stage = ItemStage.Raw) =>
            inventory.Where(i => i.OreType == ore && i.Stage == stage).Sum(i => i.Quantity);

        public static bool CanAfford(IEnumerable<InventoryItemDTO> inventory, IEnumerable<(OreType Ore, int Quantity)> cost) =>
            cost.All(c => QuantityOf(inventory, c.Ore) >= c.Quantity);

        /// <summary>Debita de um (minério, estágio); NotEnoughResources se faltar (nada é alterado nesse caso).</summary>
        public static void RemoveStage(List<InventoryItemDTO> inventory, OreType ore, ItemStage stage, int quantity)
        {
            if (QuantityOf(inventory, ore, stage) < quantity) throw new ShardsException(ShardsErrorCode.NotEnoughResources);

            inventory.First(i => i.OreType == ore && i.Stage == stage).Quantity -= quantity;
        }

        /// <summary>Debita minérios brutos; NotEnoughResources se faltar qualquer um (nada é alterado nesse caso).</summary>
        public static void Remove(List<InventoryItemDTO> inventory, IReadOnlyCollection<(OreType Ore, int Quantity)> cost)
        {
            if (!CanAfford(inventory, cost)) throw new ShardsException(ShardsErrorCode.NotEnoughResources);

            foreach ((OreType ore, int quantity) in cost)
                inventory.First(i => i.OreType == ore && i.Stage == ItemStage.Raw).Quantity -= quantity;
        }
    }
}
