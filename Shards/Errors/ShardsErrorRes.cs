namespace Shards.Errors
{
    /// <summary>Corpo de erro do Shards. Code é o nome do <see cref="ShardsErrorCode"/>, estável para o cliente mapear.</summary>
    public record ShardsErrorRes
    {
        public required string Code { get; init; }

        public required string Message { get; init; }
    }
}
