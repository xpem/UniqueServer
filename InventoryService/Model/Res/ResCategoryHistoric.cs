namespace InventoryModels.Res
{
    public record ResCategoryHistoric
    {
        public int Id { get; init; }
        public DateTime CreatedAt { get; init; }
        public int CategoryId { get; init; }
        public int? TypeId { get; init; }
        public string? TypeName { get; init; }
        public List<ResCategoryHistoricField>? Fields { get; init; }
    }

    public record ResCategoryHistoricField
    {
        public int Id { get; init; }
        public int FieldId { get; init; }
        public string? FieldName { get; init; }
        public string? UpdatedFrom { get; init; }
        public string? UpdatedTo { get; init; }
    }
}
