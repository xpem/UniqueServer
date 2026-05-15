using FinancialService.Model.Dto;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Repo
{
    public interface ITransactionCategoryRepo
    {
        Task<List<TransactionCategoryDTO>> GetByUid(int uid, DateTime updatedAt);
    }

    public class TransactionCategoryRepo(IDbContextFactory<FinancialDbctx> dbCtx) : ITransactionCategoryRepo
    {
        public async Task<List<TransactionCategoryDTO>> GetByUid(int uid, DateTime updatedAt)
        {
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            List<TransactionCategoryDTO> categories = await context.TransactionCategory
                .Where(c => c.UserId == uid && c.UpdatedAt > updatedAt)
                .ToListAsync();
            return categories;
        }
    }
}
