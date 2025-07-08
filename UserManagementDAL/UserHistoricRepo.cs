using Microsoft.EntityFrameworkCore;
using UserManagementModels;

namespace UserManagementRepo
{
    public class UserHistoricRepo(UserManagementDbContext userManagementDbContext) : IUserHistoricRepo
    {
        public async Task<int> AddAsync(UserHistoric userHistoric)
        {
            await userManagementDbContext.UserHistoric.AddAsync(userHistoric);

            return await userManagementDbContext.SaveChangesAsync();
        }

        public async Task DeleteAllAsync(int uid)
        {
            await userManagementDbContext.UserHistoric.Where(x => x.UserId.Equals(uid)).ExecuteDeleteAsync();
        }
    }
}
