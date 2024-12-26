using BaseModels;
using BookshelfServices;
using UserManagementModels;
using UserManagementModels.Request.User;
using UserManagementRepo;
using UserManagementService.Functions;

namespace UserManagementService
{
    public class UserDataDeleteService(IUserRepo userRepo, IEncryptionService encryptionService,
        IBookService bookService, IBookHistoricService bookHistoricService, IUserHistoricRepo userHistoricRepo) : IUserDataDeleteService
    {
        public async Task<BaseResponse> DeleteAsync(ReqUserDataExclusion reqUserDataExclusion)
        {
            string? validateError = reqUserDataExclusion.Validate();
            if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(null, validateError);

            User? userResp = await userRepo.GetByEmailAndPasswordAsync(reqUserDataExclusion.Email, encryptionService.Encrypt(reqUserDataExclusion.Password));

            if (userResp is null) return new BaseResponse(null, "User/Password incorrect");

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

            return new BaseResponse(null);
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
