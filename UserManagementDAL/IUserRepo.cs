using UserModels;

namespace UserManagementDAL
{
    public interface IUserRepo
    {
        Task<int> CreateAsync(User user);
        Task<int> UpdateAsync(User user);
        Task<User?> GetByEmailAndPasswordAsync(string email, string encryptedPassword);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int uid);
        Task<User?> GetByNameOrEmailAsync(string name, string email);
    }
}