using BaseModels;
using UserModels.Request.User;

namespace UserService
{
    public interface IUserService
    {
        Task<BaseResponse> CreateAsync(ReqUser reqUser);

        Task<BaseResponse> GenerateTokenAsync(ReqUserSession reqUserSession);

        Task<BaseResponse> GetByIdAsync(int uid);

        Task<BaseResponse> SendRecoverPasswordEmailAsync(ReqUserEmail reqUserEmail);

        Task<BaseResponse> UpdatePasswordAsync(ReqRecoverPassword reqRecoverPassword, int uid);
    }
}