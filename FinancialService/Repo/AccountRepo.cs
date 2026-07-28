using FinancialService.Model.DTO;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Repo
{
    public interface IAccountRepo
    {
        Task Add(AccountDTO account);
        Task Update(AccountDTO account);
        Task<AccountDTO?> GetByIdAsync(int id, int uid);
        Task<AccountDTO?> FindByAccountIdAsync(Guid accountId, int userId);
        Task<List<AccountDTO>> GetAllAsync(int uid);
        Task<List<AccountDTO>> GetUpdatedAfterAsync(int uid, DateTime updatedAt, int page, int pageSize);
    }

    public class AccountRepo(IDbContextFactory<FinancialDbctx> dbCtx) : IAccountRepo
    {
        public async Task<AccountDTO?> GetByIdAsync(int id, int uid)
        {
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            return await context.Account.FirstOrDefaultAsync(a => a.Id == id && a.UserId == uid);
        }

        public async Task<AccountDTO?> FindByAccountIdAsync(Guid accountId, int userId)
        {
            if (accountId == Guid.Empty) return null;

            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            return await context.Account.FirstOrDefaultAsync(a => a.AccountId == accountId && a.UserId == userId);
        }

        public async Task<List<AccountDTO>> GetAllAsync(int uid)
        {
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            return await context.Account.Where(a => a.UserId == uid).ToListAsync();
        }

        public async Task<List<AccountDTO>> GetUpdatedAfterAsync(int uid, DateTime updatedAt, int page, int pageSize)
        {
            DateTime updatedAtUtc = DateTime.SpecifyKind(updatedAt, DateTimeKind.Utc);
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            return await context.Account
                .Where(a => a.UserId == uid && a.UpdatedAt > updatedAtUtc)
                .OrderBy(a => a.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task Add(AccountDTO account)
        {
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            context.Account.Add(account);
            await context.SaveChangesAsync();
        }

        public async Task Update(AccountDTO account)
        {
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            context.Account.Update(account);
            await context.SaveChangesAsync();
        }
    }
}
