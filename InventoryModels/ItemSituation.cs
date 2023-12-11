namespace InventoryModels
{
    public class ItemSituation : InventoryBaseModel
    {
        public required int? Sequence { get; set; }

        public required bool Hand { get; set; } = false;
    }
}
