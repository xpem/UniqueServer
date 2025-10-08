using InventoryModels.DTOs;
using InventoryModels.Res.Bases;

namespace InventoryModels.Res.Item
{
    public record ResItemSituation : ResTextTypeBase
    {
        public required SituationType Type { get; set; }

        public int Quantity { get; set; } = 0;
    }
}
