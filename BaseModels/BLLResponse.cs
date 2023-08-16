namespace BaseModels
{
    public record BLLResponse
    {
        public object? Content { get; init; }

        public ErrorMessage? Error { get; init; }
    }

    public record ErrorMessage
    {
        public string? Error { get; init; }
    }
}