using UserModels;

namespace UserManagementDAL
{
    public interface IUserDAL
    {
        Task<int> ExecuteCreateUserAsync(User user);
        Task<int> ExecuteUpdateUserAsync(User user);
        Task<User?> GetUserByEmailAndPassword(string email, string encryptedPassword);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int uid);
        Task<User?> GetUserByNameOrEmailAsync(string name, string email);
    }
}