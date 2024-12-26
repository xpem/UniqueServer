using BaseModels;
using UserManagementModels.Request.User;

namespace UserManagementService
{
    public interface IUserDataDeleteService
    {
        Task<BaseResponse> DeleteAsync(ReqUserDataExclusion reqUserDataExclusion);
    }
}