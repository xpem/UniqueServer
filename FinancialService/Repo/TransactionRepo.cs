using FinancialService.Model.DTO;
using FinancialService.Model.Req;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Repo
{
    public interface ITransactionRepo
    {
        Task AddAsync(TransactionDTO transaction);
        Task UpdateAsync(TransactionDTO transaction);
        Task<List<TransactionDTO>> GetByUpdatedAtAsync(int uid, DateTime updatedAt);
        Task<TransactionDTO?> GetByIdAsync(int id, int uid);
        Task<decimal> GetSumByAccountIdAsync(int accountId);
        Task<TransactionDTO?> FindDuplicateAsync(int uid, TransactionReq req);
    }

    public class TransactionRepo(IDbContextFactory<FinancialDbctx> dbCtx) : ITransactionRepo
    {
        public async Task AddAsync(TransactionDTO transaction)
        {
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            context.Transaction.Add(transaction);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TransactionDTO transaction)
        {
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            context.Transaction.Update(transaction);
            await context.SaveChangesAsync();
        }

        public async Task<List<TransactionDTO>> GetByUpdatedAtAsync(int uid, DateTime updatedAt)
        {
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            return await context.Transaction
                .Where(t => t.UserId == uid && t.UpdatedAt > updatedAt)
                .ToListAsync();
        }

        public async Task<TransactionDTO?> GetByIdAsync(int id, int uid)
        {
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            return await context.Transaction
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == uid);
        }

        public async Task<decimal> GetSumByAccountIdAsync(int accountId)
        {
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            return await context.Transaction
                .Where(t => t.AccountId == accountId && !t.Inactive)
                .SumAsync(t => t.Amount);
        }

        /// <summary>
        /// Detecta duplicatas baseado em InstallmentId (para parcelamentos) ou combinação de campos chave.
        /// Retorna a transação duplicada existente, ou null se não houver duplicata.
        /// </summary>
        public async Task<TransactionDTO?> FindDuplicateAsync(int uid, TransactionReq req)
        {
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();

            // Para parcelamentos: usa InstallmentId + Installment como chave única
            if (req.InstallmentId.HasValue && req.Installment.HasValue)
            {
                var duplicate = await context.Transaction
                    .FirstOrDefaultAsync(t =>
                        t.UserId == uid
                        && t.InstallmentId == req.InstallmentId
                        && t.Installment == req.Installment);

                if (duplicate != null)
                    return duplicate;
            }

            // Para transações normais: combina Description, Date, Amount, AccountId como identificador
            // Verifica se já existe uma transação criada nos últimos 5 minutos com os mesmos dados
            DateTime recentThreshold = DateTime.UtcNow.AddMinutes(-5);
            var potentialDuplicate = await context.Transaction
                .FirstOrDefaultAsync(t =>
                    t.UserId == uid
                    && t.Description == req.Description
                    && t.Date == req.Date
                    && t.Amount == req.Amount
                    && t.AccountId == req.AccountId
                    && t.Type == req.Type
                    && t.CreatedAt >= recentThreshold);

            return potentialDuplicate;
        }
    }
}
