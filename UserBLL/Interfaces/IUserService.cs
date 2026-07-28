using BaseModels;
using UserManagementModels.Request.User;

namespace UserManagementService.Interfaces
{
    public interface IUserService
    {
        Task<BaseResp> CreateAsync(ReqUser reqUser);

        Task<BaseResp> GenerateTokenAsync(ReqUserSession reqUserSession);

        Task<BaseResp> RefreshTokenAsync(ReqRefreshToken reqRefreshToken);

        Task<BaseResp> GetByIdAsync(int uid);
        Task<BaseResp> GoogleAuthAsync(string idToken);
        Task<string> GoogleAuthStartAsync(string redirectUri);
        Task<(string appRedirectUri, string? error)> GoogleAuthCallbackAsync(string code, string redirectUri);
        Task<BaseResp> SendRecoverPasswordEmailAsync(ReqUserEmail reqUserEmail);

        Task<BaseResp> UpdatePasswordAsync(ReqRecoverPassword reqRecoverPassword, int uid);
    }
}