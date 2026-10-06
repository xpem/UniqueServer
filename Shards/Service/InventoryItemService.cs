using Shards.Errors;
using Shards.Model.DTO;
using Shards.Repo;

namespace Shards.Service
{
    /// <summary>
    /// Operações sobre o inventário do jogador, sobre entidades já carregadas e rastreadas pelo DbContext do comando
    /// (quem chama faz o SaveChanges junto com o resto do comando).
    /// </summary>
    public static class InventoryItemService
    {
        /// <summary>Soma minérios ao inventário, criando a linha do tipo se ainda não existir.</summary>
        public static void Add(ShardsDbctx ctx, List<InventoryItemDTO> inventory, int playerId, OreType ore, int quantity)
        {
            if (quantity <= 0) return;

            InventoryItemDTO? item = inventory.FirstOrDefault(i => i.OreType == ore);
            if (item is null)
            {
                item = new InventoryItemDTO { PlayerId = playerId, OreType = ore };
                inventory.Add(item);
                ctx.InventoryItem.Add(item);
            }

            item.Quantity += quantity;
        }

        public static int QuantityOf(IEnumerable<InventoryItemDTO> inventory, OreType ore) =>
            inventory.Where(i => i.OreType == ore).Sum(i => i.Quantity);

        public static bool CanAfford(IEnumerable<InventoryItemDTO> inventory, IEnumerable<(OreType Ore, int Quantity)> cost) =>
            cost.All(c => QuantityOf(inventory, c.Ore) >= c.Quantity);

        /// <summary>Debita minérios; NotEnoughResources se faltar qualquer um (nada é alterado nesse caso).</summary>
        public static void Remove(List<InventoryItemDTO> inventory, IReadOnlyCollection<(OreType Ore, int Quantity)> cost)
        {
            if (!CanAfford(inventory, cost)) throw new ShardsException(ShardsErrorCode.NotEnoughResources);

            foreach ((OreType ore, int quantity) in cost)
                inventory.First(i => i.OreType == ore).Quantity -= quantity;
        }
    }
}
