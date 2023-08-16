using BaseModels;
using UserModels.Request.User;

namespace UserBLL
{
    public interface IUserBLL
    {
        Task<BLLResponse> CreateUser(ReqUser reqUser);

        Task<BLLResponse> GenerateUserToken(ReqUserSession reqUserSession);

        Task<BLLResponse> GetUserById(int uid);

        Task<BLLResponse> SendRecoverPasswordEmail(ReqUserEmail reqUserEmail);

        Task<BLLResponse> UpdatePassword(ReqRecoverPassword reqRecoverPassword, int uid);
    }
}