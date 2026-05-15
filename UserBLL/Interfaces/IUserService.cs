using BaseModels;
using UserManagementModels.Request.User;

namespace UserManagementService.Interfaces
{
    public interface IUserService
    {
        Task<BaseResp> CreateAsync(ReqUser reqUser);

        Task<BaseResp> GenerateTokenAsync(ReqUserSession reqUserSession);

        Task<BaseResp> GetByIdAsync(int uid);
        Task<BaseResp> GoogleAuthAsync(string name, string email);
        Task<BaseResp> SendRecoverPasswordEmailAsync(ReqUserEmail reqUserEmail);

        Task<BaseResp> UpdatePasswordAsync(ReqRecoverPassword reqRecoverPassword, int uid);
    }
}