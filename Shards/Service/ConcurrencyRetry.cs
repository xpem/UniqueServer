using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shards.Errors;

namespace Shards.Service
{
    public static class ConcurrencyRetry
    {
        public const int MaxAttempts = 3;

        /// <summary>
        /// Executa um comando do jogo e o repete se outra requisição alterou as mesmas linhas no meio (token xmin)
        /// ou criou a mesma linha única antes (ex.: primeiro minério de um tipo no inventário).
        /// A ação deve criar um DbContext novo a cada tentativa, ler o estado e gravar. Esgotadas as tentativas,
        /// devolve ConcurrencyConflict (409) para o cliente repetir.
        /// </summary>
        public static async Task<T> RunAsync<T>(Func<Task<T>> action)
        {
            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex) when (IsConflict(ex) && attempt < MaxAttempts)
                {
                    // outra requisição ganhou a corrida: relê o estado e tenta de novo
                }
                catch (Exception ex) when (IsConflict(ex))
                {
                    throw new ShardsException(ShardsErrorCode.ConcurrencyConflict);
                }
            }
        }

        private static bool IsConflict(Exception ex) => ex is DbUpdateConcurrencyException || IsUniqueViolation(ex);

        /// <summary>Violação de índice único do Postgres (SQLSTATE 23505): outra requisição criou a mesma linha antes.</summary>
        public static bool IsUniqueViolation(Exception ex) =>
            ex is DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } };
    }
}
