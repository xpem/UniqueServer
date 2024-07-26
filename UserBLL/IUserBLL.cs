using BaseModels;
using UserModels.Request.User;

namespace UserBLL
{
    public interface IUserBLL
    {
        Task<BaseResponse> CreateUser(ReqUser reqUser);

        Task<BaseResponse> GenerateUserToken(ReqUserSession reqUserSession);

        Task<BaseResponse> GetUserById(int uid);

        Task<BaseResponse> SendRecoverPasswordEmail(ReqUserEmail reqUserEmail);

        Task<BaseResponse> UpdatePassword(ReqRecoverPassword reqRecoverPassword, int uid);
    }
}