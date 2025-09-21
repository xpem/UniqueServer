namespace InventoryModels.Res.Item
{
    public record ResItemConfigs
    {
        public List<ResCategoryWithSubCategories> Categories { get; init; } = new();

        public List<ResAcquisitionType> AcquisitionTypes { get; init; } = new();

        public List<ResItemSituation> ItemSituations { get; init; } = new();

        public List<string> LastPurchaseStores { get; init; } = new();
    }
}
