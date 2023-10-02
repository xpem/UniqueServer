using UserModels;

namespace UserManagementDAL
{
    public class UserHistoricDAL : IUserHistoricDAL
    {
        private readonly UserManagementDbContext userManagementDbContext;

        public UserHistoricDAL(UserManagementDbContext userManagementDbContext)
        {
            this.userManagementDbContext = userManagementDbContext;
        }

        public async Task<int> ExecuteAddUserHistoric(UserHistoric userHistoric)
        {
            await userManagementDbContext.UserHistoric.AddAsync(userHistoric);

            return await userManagementDbContext.SaveChangesAsync();
        }
    }
}
