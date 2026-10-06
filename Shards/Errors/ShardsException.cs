namespace Shards.Errors
{
    /// <summary>Violação de regra de negócio. O controller a traduz para { code, message } com o status HTTP do código.</summary>
    public class ShardsException(ShardsErrorCode code, string? message = null)
        : Exception(message ?? ShardsErrors.DefaultMessage(code))
    {
        public ShardsErrorCode Code { get; } = code;

        public ShardsErrorRes ToResponse() => new() { Code = Code.ToString(), Message = Message };
    }
}
