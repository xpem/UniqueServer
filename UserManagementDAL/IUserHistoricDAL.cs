using UserModels;

namespace UserManagementDAL
{
    public interface IUserHistoricDAL
    {
        Task<int> ExecuteAddUserHistoric(UserHistoric userHistoric);
    }
}