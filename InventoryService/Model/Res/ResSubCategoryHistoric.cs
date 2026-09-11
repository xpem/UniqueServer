namespace InventoryModels.Res
{
    public record ResSubCategoryHistoric
    {
        public int Id { get; init; }
        public DateTime CreatedAt { get; init; }
        public int SubCategoryId { get; init; }
        public int? TypeId { get; init; }
        public string? TypeName { get; init; }
        public List<ResSubCategoryHistoricField>? Fields { get; init; }
    }

    public record ResSubCategoryHistoricField
    {
        public int Id { get; init; }
        public int FieldId { get; init; }
        public string? FieldName { get; init; }
        public string? UpdatedFrom { get; init; }
        public string? UpdatedTo { get; init; }
    }
}
