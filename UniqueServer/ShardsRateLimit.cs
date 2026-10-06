using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace UniqueServer
{
    /// <summary>
    /// Política de rate limit do Shards: janela fixa por jogador (claim "uid"), com fallback por IP.
    /// A política "fixed" é um único balde para toda a API; o jogo precisa de um limite por usuário e mais folgado,
    /// já que um jogador alterna minerar, coletar e consultar o estado em sequência.
    /// </summary>
    public static class ShardsRateLimit
    {
        public const string PolicyName = "shards";

        public const int PermitLimit = 30;

        public static readonly TimeSpan Window = TimeSpan.FromSeconds(10);

        public static RateLimiterOptions AddShardsPolicy(this RateLimiterOptions options)
        {
            options.AddPolicy(PolicyName, context =>
            {
                string key = context.User.FindFirst("uid")?.Value
                    ?? context.Connection.RemoteIpAddress?.ToString()
                    ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = PermitLimit,
                    Window = Window,
                    QueueLimit = 0,
                });
            });

            return options;
        }
    }
}
