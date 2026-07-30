using UserManagementModels;

namespace UserManagementRepo
{
    public interface IUserHistoricRepo
    {
        Task<int> AddAsync(UserHistoric userHistoric);

        Task DeleteAllAsync(int uid);
    }
}