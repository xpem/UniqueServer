namespace BookshelfModels.Response
{
    public record ResBookHistoric
    {
        public int Id { get; init; }

        public DateTime CreatedAt { get; init; }

        public int? TypeId { get; init; }

        public string? TypeName { get; init; }

        public int BookId { get; init; }

        public List<ResBookHistoricItem>? BookHistoricItems { get; init; }
    }

    public record ResBookHistoricItem
    {
        public int Id { get; init; }

        public DateTime CreatedAt { get; init; }

        public int BookFieldId { get; init; }

        public string? BookFieldName { get; init; }

        public string? UpdatedFrom { get; init; }

        public string? UpdatedTo { get; init; }
    }
}
