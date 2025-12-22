namespace InventoryModels.Res.Item
{
    public record ResItemConfigs
    {
        public List<ResCategoryWithSubCategories> Categories { get; init; } = [];

        public List<ResAcquisitionType> AcquisitionTypes { get; init; } = [];

        public List<ResItemSituation> ItemSituations { get; init; } = [];

        public List<string> LastPurchaseStores { get; init; } = [];
    }
}
