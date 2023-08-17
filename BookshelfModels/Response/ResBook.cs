namespace BookshelfModels.Response
{
    public record ResBook
    {
        public int Id { get; init; }

        public string? Cover { get; init; }

        public string? Title { get; init; }

        public string? Subtitle { get; init; }

        public string? Authors { get; init; }

        public int? Volume { get; init; }

        public int? Pages { get; init; }

        public int? Year { get; init; }

        public int Status { get; init; }

        public int? Score { get; init; }

        public string? Comment { get; init; }

        public string? Genre { get; init; }

        public string? Isbn { get; init; }

        public string? GoogleId { get; init; }

        public bool Inactive { get; init; }

        public DateTime CreatedAt { get; init; }

        public required DateTime UpdatedAt { get; init; }
    }
}
