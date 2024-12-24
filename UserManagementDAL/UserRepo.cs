using Microsoft.EntityFrameworkCore;
using UserManagementModels;

namespace UserManagementRepo
{
    public class UserRepo(UserManagementDbContext userManagementDbContext) : IUserRepo
    {
        private readonly UserManagementDbContext userManagementDbContext = userManagementDbContext;

        public async Task<int> CreateAsync(User user)
        {
            await userManagementDbContext.User.AddAsync(user);

            return await userManagementDbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(User user)
        {
            userManagementDbContext.User.Update(user);

            return await userManagementDbContext.SaveChangesAsync();
        }

        public async Task<User?> GetByIdAsync(int uid) => await userManagementDbContext.User.FirstOrDefaultAsync(x => x.Id.Equals(uid));

        public async Task<User?> GetByEmailAsync(string email) => await userManagementDbContext.User.FirstOrDefaultAsync(x => x.Email.Equals(email));

        public async Task<User?> GetByEmailAndPasswordAsync(string email, string encryptedPassword) =>
            await userManagementDbContext.User.FirstOrDefaultAsync(x => x.Email == email && x.Password == encryptedPassword);

        public async Task<User?> GetByNameOrEmailAsync(string name, string email) => await userManagementDbContext.User.FirstOrDefaultAsync(x => x.Name.Equals(name) || x.Email.Equals(email));


    }
}
