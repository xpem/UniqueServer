using FinancialService.Model.DTO;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Repo
{
    public interface IAccountRepo
    {
        Task Add(AccountDTO account);
        Task<AccountDTO?> GetAsync(int uid);
        Task<AccountDTO?> GetIfUpdatedAfterAsync(int uid, DateTime updatedAt);
        Task Update(AccountDTO account);
    }

    public class AccountRepo(IDbContextFactory<FinancialDbctx> dbCtx) : IAccountRepo
    {
        public async Task<AccountDTO?> GetAsync(int uid)
        {
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            return await context.Account.FirstOrDefaultAsync(a => a.UserId == uid);
        }

        public async Task<AccountDTO?> GetIfUpdatedAfterAsync(int uid, DateTime updatedAt)
        {
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            return await context.Account.FirstOrDefaultAsync(a => a.UserId == uid && a.UpdatedAt > updatedAt);
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
