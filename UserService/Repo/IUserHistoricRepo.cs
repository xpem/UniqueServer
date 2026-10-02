using UserManagementService.Model;

namespace UserManagementService.Repo
{
    public interface IUserHistoricRepo
    {
        Task<int> AddAsync(UserHistoric userHistoric);

        Task DeleteAllAsync(int uid);
    }
}