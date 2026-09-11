namespace InventoryModels.Res.Item
{
    public record ResItemHistoric
    {
        public int Id { get; init; }
        public DateTime CreatedAt { get; init; }
        public int ItemId { get; init; }
        public int? TypeId { get; init; }
        public string? TypeName { get; init; }
        public List<ResItemHistoricField>? Fields { get; init; }
    }

    public record ResItemHistoricField
    {
        public int Id { get; init; }
        public int FieldId { get; init; }
        public string? FieldName { get; init; }
        public string? UpdatedFrom { get; init; }
        public string? UpdatedTo { get; init; }
    }
}
