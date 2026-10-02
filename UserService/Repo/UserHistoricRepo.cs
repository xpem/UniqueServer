using Microsoft.EntityFrameworkCore;
using UserManagementService.Model;

namespace UserManagementService.Repo
{
    public class UserHistoricRepo(IDbContextFactory<UserManagementDbCtx> dbCtx) : IUserHistoricRepo
    {
        public async Task<int> AddAsync(UserHistoric userHistoric)
        {
            await using var dbContext = await dbCtx.CreateDbContextAsync();
            await dbContext.UserHistoric.AddAsync(userHistoric);

            return await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAllAsync(int uid)
        {
            await using var dbContext = await dbCtx.CreateDbContextAsync();
            await dbContext.UserHistoric.Where(x => x.UserId.Equals(uid)).ExecuteDeleteAsync();
        }
    }
}
