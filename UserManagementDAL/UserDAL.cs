using Microsoft.EntityFrameworkCore;
using UserModels;

namespace UserManagementDAL
{
    public class UserDAL : IUserDAL
    {
        private readonly UserManagementDbContext userManagementDbContext;

        public UserDAL(UserManagementDbContext userManagementDbContext)
        {
            this.userManagementDbContext = userManagementDbContext;
        }

        public async Task<int> ExecuteCreateUserAsync(User user)
        {
            await userManagementDbContext.User.AddAsync(user);

            return await userManagementDbContext.SaveChangesAsync();
        }

        public async Task<int> ExecuteUpdateUserAsync(User user)
        {
            userManagementDbContext.User.Update(user);

            return await userManagementDbContext.SaveChangesAsync();
        }

        public async Task<User?> GetUserByIdAsync(int uid) => await userManagementDbContext.User.FirstOrDefaultAsync(x => x.Id.Equals(uid));

        public async Task<User?> GetUserByEmailAsync(string email) => await userManagementDbContext.User.FirstOrDefaultAsync(x => x.Email.Equals(email));

        public async Task<User?> GetUserByEmailAndPassword(string email, string encryptedPassword) =>
            await userManagementDbContext.User.FirstOrDefaultAsync(x => x.Email == email && x.Password == encryptedPassword);

        public async Task<User?> GetUserByNameOrEmailAsync(string name, string email) => await userManagementDbContext.User.FirstOrDefaultAsync(x => x.Name.Equals(name) || x.Email.Equals(email));


    }
}
