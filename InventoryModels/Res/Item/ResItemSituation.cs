using InventoryModels.Res.Bases;

namespace InventoryModels.Res.Item
{
    public record ResItemSituation : ResTextTypeBase
    {
        public required SituationType Type { get; set; }
    }
}
