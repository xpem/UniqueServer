namespace InventoryModels.DTOs
{
    public class ItemSituation : InventoryBaseModel
    {
        public required int Sequence { get; set; }

        public required SituationType Type { get; set; }
    }

    public enum SituationType { In, Out }
}
