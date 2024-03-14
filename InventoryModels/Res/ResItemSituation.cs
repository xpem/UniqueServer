using InventoryModels.Res.Bases;

namespace InventoryModels.Res
{
    public record ResItemSituation : ResTextTypeBase
    {
        public required SituationType Type { get; set; }
    }
}
