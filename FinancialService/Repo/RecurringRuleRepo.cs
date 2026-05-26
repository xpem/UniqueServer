using FinancialService.Model.DTO;
using Microsoft.EntityFrameworkCore;

namespace FinancialService.Repo
{
    public interface IRecurringRuleRepo
    {
        Task<RecurringRuleDTO> AddAsync(RecurringRuleDTO rule);
        Task UpdateAsync(RecurringRuleDTO rule);
        Task<List<RecurringRuleDTO>> GetByUpdatedAtAsync(int uid, DateTime updatedAt);
    }

    public class RecurringRuleRepo(IDbContextFactory<FinancialDbctx> dbCtx) : IRecurringRuleRepo
    {
        public async Task<RecurringRuleDTO> AddAsync(RecurringRuleDTO rule)
        {
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            context.RecurringRule.Add(rule);
            await context.SaveChangesAsync();
            return rule;
        }

        public async Task UpdateAsync(RecurringRuleDTO rule)
        {
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            context.RecurringRule.Update(rule);
            await context.SaveChangesAsync();
        }

        public async Task<List<RecurringRuleDTO>> GetByUpdatedAtAsync(int uid, DateTime updatedAt)
        {
            using FinancialDbctx context = await dbCtx.CreateDbContextAsync();
            return await context.RecurringRule
                .Where(r => r.UserId == uid && r.UpdatedAt > updatedAt)
                .ToListAsync();
        }
    }
}
