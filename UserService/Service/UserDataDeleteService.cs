using BaseModels;
using BookshelfServices;
using UserManagementService.Model;
using UserManagementService.Model.Request.User;
using UserManagementService.Repo;

namespace UserManagementService.Service
{
    public class UserDataDeleteService(IUserRepo userRepo, IUserService userService,
        IBookService bookService, IBookHistoricService bookHistoricService, IUserHistoricRepo userHistoricRepo) : IUserDataDeleteService
    {
        public async Task<BaseResp> DeleteAsync(ReqUserDataExclusion reqUserDataExclusion)
        {
            string? validateError = reqUserDataExclusion.Validate();
            if (!string.IsNullOrEmpty(validateError)) return new BaseResp(ErrorCode.InvalidObject, validateError);

            User? userResp = await userService.VerifyPasswordAsync(reqUserDataExclusion.Email, reqUserDataExclusion.Password);

            if (userResp is null) return new BaseResp(ErrorCode.InvalidUserPasswordLogin, "User/Password incorrect");

            if (reqUserDataExclusion.UserAccount is not null)
            {
                await DeleteBookshelfData(userResp.Id);

                await DeleteUserData(userResp.Id);
            }
            else if (reqUserDataExclusion.UserDataBookshelf is not null)
            {
                await DeleteBookshelfData(userResp.Id);
            }
            else throw new Exception("Invalid request");

            return new BaseResp(null);
        }

        private async Task DeleteBookshelfData(int uid)
        {
            await bookHistoricService.DeleteAllAsync(uid);
            await bookService.DeleteAllAsync(uid);
        }

        private async Task DeleteUserData(int uid)
        {
            await userHistoricRepo.DeleteAllAsync(uid);
            await userRepo.DeleteAsync(uid);
        }
    }


}
