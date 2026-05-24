using FinancialService.Model.DTO;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Repo
{
    public interface ITransactionRepo
    {
        Task AddAsync(TransactionDTO transaction);
        Task<List<TransactionDTO>> GetByUpdatedAtAsync(int uid, DateTime updatedAt);
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
    }
}
