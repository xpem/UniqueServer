using BaseModels;
using UserManagementModels.Request.User;

namespace UserManagementService.Interfaces
{
    public interface IUserDataDeleteService
    {
        Task<BaseResponse> DeleteAsync(ReqUserDataExclusion reqUserDataExclusion);
    }
}