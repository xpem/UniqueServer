using BaseModels;
using UserManagementService.Model.Request.User;

namespace UserManagementService.Service
{
    public interface IUserDataDeleteService
    {
        Task<BaseResp> DeleteAsync(ReqUserDataExclusion reqUserDataExclusion);
    }
}