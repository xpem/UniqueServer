using BaseModels;
using UserManagementService.Model;
using UserManagementService.Model.Request.User;

namespace UserManagementService.Service
{
    public interface IUserService
    {
        Task<BaseResp> CreateAsync(ReqUser reqUser);

        Task<BaseResp> GenerateTokenAsync(ReqUserSession reqUserSession);

        /// <summary>
        /// Verifica email/senha (suporta migração transparente de senhas no formato legado).
        /// Usado também por fluxos que precisam reautenticar o usuário (ex: exclusão de dados).
        /// </summary>
        Task<User?> VerifyPasswordAsync(string email, string password);

        Task<BaseResp> RefreshTokenAsync(ReqRefreshToken reqRefreshToken);

        Task<BaseResp> GetByIdAsync(int uid);
        Task<BaseResp> GoogleAuthAsync(string idToken);
        Task<string> GoogleAuthStartAsync();
        Task<(string appRedirectUri, string? error)> GoogleAuthCallbackAsync(string code);
        Task<BaseResp> SendRecoverPasswordEmailAsync(ReqUserEmail reqUserEmail);

        Task<BaseResp> UpdatePasswordAsync(ReqRecoverPassword reqRecoverPassword, int uid);
    }
}