using UserManagementModels;

namespace UserManagementRepo
{
    public interface IUserRepo
    {
        Task<int> CreateAsync(User user);

        Task<int> UpdateAsync(User user);

        Task<User?> GetByEmailAndPasswordAsync(string email, string encryptedPassword);

        Task<User?> GetByEmailAsync(string email);

        Task<User?> GetByIdAsync(int uid);

        Task<User?> GetByNameOrEmailAsync(string name, string email);

        Task DeleteAsync(int uid);
    }
}