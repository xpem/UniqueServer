using FinancialService.Model.DTO;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Repo
{
    public interface ITransactionCategoryRepo
    {
        Task<List<TransactionCategoryDTO>> GetByUid(int uid, DateTime updatedAt);
        Task<TransactionCategoryDTO?> FindByCategoryIdAsync(Guid categoryId, int userId);
        Task<TransactionCategoryDTO> AddAsync(TransactionCategoryDTO dto);
        Task UpdateAsync(TransactionCategoryDTO dto);
    }

    public class TransactionCategoryRepo(IDbContextFactory<FinancialDbctx> dbCtx) : ITransactionCategoryRepo
    {
        public async Task<List<TransactionCategoryDTO>> GetByUid(int uid, DateTime updatedAt)
        {
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            List<TransactionCategoryDTO> categories = await context.TransactionCategory
                .Where(c => (c.UserId == uid || c.SystemDefault) && c.UpdatedAt > updatedAt)
                .ToListAsync();
            return categories;
        }

        public async Task<TransactionCategoryDTO?> FindByCategoryIdAsync(Guid categoryId, int userId)
        {
            if (categoryId == Guid.Empty) return null;

            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            return await context.TransactionCategory
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.UserId == userId);
        }

        public async Task<TransactionCategoryDTO> AddAsync(TransactionCategoryDTO dto)
        {
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            context.TransactionCategory.Add(dto);
            await context.SaveChangesAsync();
            return dto;
        }

        public async Task UpdateAsync(TransactionCategoryDTO dto)
        {
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            context.TransactionCategory.Update(dto);
            await context.SaveChangesAsync();
        }
    }
}
