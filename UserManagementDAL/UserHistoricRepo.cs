using UserManagementModels;

namespace UserManagementRepo
{
    public class UserHistoricRepo(UserManagementDbContext userManagementDbContext) : IUserHistoricRepo
    {
        public async Task<int> ExecuteAddUserHistoric(UserHistoric userHistoric)
        {
            await userManagementDbContext.UserHistoric.AddAsync(userHistoric);

            return await userManagementDbContext.SaveChangesAsync();
        }
    }
}
