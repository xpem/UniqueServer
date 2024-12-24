using UserManagementModels;

namespace UserManagementRepo
{
    public interface IUserHistoricRepo
    {
        Task<int> ExecuteAddUserHistoric(UserHistoric userHistoric);
    }
}