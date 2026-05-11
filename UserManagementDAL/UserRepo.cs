using Microsoft.EntityFrameworkCore;
using UserManagementModels;

namespace UserManagementRepo
{
    public class UserRepo(IDbContextFactory<UserManagementDbCtx> dbCtx) : IUserRepo
    {
        public async Task<int> CreateAsync(User user)
        {
            await using var dbContext = await dbCtx.CreateDbContextAsync();
            dbContext.ChangeTracker?.Clear();
            await dbContext.User.AddAsync(user);

            return await dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(User user)
        {
            await using var dbContext = await dbCtx.CreateDbContextAsync();
            dbContext.ChangeTracker?.Clear();

            dbContext.User.Update(user);

            return await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int uid)
        {
            await using var dbContext = await dbCtx.CreateDbContextAsync();
            dbContext.ChangeTracker?.Clear();
            await dbContext.User.Where(x => x.Id.Equals(uid)).ExecuteDeleteAsync();
        }


        public async Task<User?> GetByIdAsync(int uid) {
            await using var dbContext = await dbCtx.CreateDbContextAsync();
            return await dbContext.User.FirstOrDefaultAsync(x => x.Id.Equals(uid)); 
        }

        public async Task<User?> GetByEmailAsync(string email) {
            await using var dbContext = await dbCtx.CreateDbContextAsync();
            return await dbContext.User.FirstOrDefaultAsync(x => x.Email.Equals(email));
        }

        public async Task<User?> GetByEmailAndPasswordAsync(string email, string encryptedPassword) {
            await using var dbContext = await dbCtx.CreateDbContextAsync();
            return await dbContext.User.FirstOrDefaultAsync(x => x.Email == email && x.Password == encryptedPassword);
        }

        //public async Task<User?> GetByNameOrEmailAsync(string name, string email) => await dbContext.User.FirstOrDefaultAsync(x => x.Name.Equals(name) || x.Email.Equals(email));


    }
}
