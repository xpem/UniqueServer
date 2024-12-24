using BaseModels;
using UserManagementModels.Response;
using UserModels;
using UserModels.Request.User;
using UserModels.Response;
using UserService;
using UserManagementService.Functions;

namespace UserManagementBLL
{
    public class UserService(UserManagementDAL.IUserRepo userRepo, UserManagementDAL.IUserHistoricDAL userHistoricRepo,
        ISendRecoverPasswordEmailService sendRecoverPasswordEmailService, IEncryptionService encryptionService,
        IJwtTokenService jwtTokenService) : IUserService
    {
        public async Task<BaseResponse> CreateAsync(ReqUser reqUser)
        {
            string? validateError = reqUser.Validate();

            if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(null, validateError);

            User user = new() { Name = reqUser.Name, Email = reqUser.Email, Password = reqUser.Password, CreatedAt = DateTime.Now };

            string? existingUserMessage = await ValidateExistingUserAsync(user);
            if (existingUserMessage != null) { return new BaseResponse(null, existingUserMessage); }

            if (user.Password != null)
                user.Password = encryptionService.Encrypt(user.Password);
            else throw new NullReferenceException("Password do usuario nulo");

            await userRepo.CreateAsync(user);

            ResUser? resUser;

            if (user?.Id is not null)
                resUser = new() { Id = user.Id, Name = user.Name, Email = user.Email, CreatedAt = user.CreatedAt };
            else throw new NullReferenceException("Id do usuário nulo");

            return new BaseResponse(resUser);
        }

        public async Task<BaseResponse> GetByIdAsync(int uid)
        {
            //todo - utilizar tmbm o email?
            User? userResp = await userRepo.GetByIdAsync(uid);

            if (userResp == null)
                return new BaseResponse(null, "User not found");

            return new BaseResponse(new ResUser() { Id = userResp.Id, Name = userResp.Name, Email = userResp.Email, CreatedAt = userResp.CreatedAt });
        }

        public async Task<BaseResponse> SendRecoverPasswordEmailAsync(ReqUserEmail reqUserEmail)
        {
            string? validateError = reqUserEmail.Validate();

            if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(null, validateError);

            User? userResp = await userRepo.GetByEmailAsync(reqUserEmail.Email);

            if (userResp != null)
            {
                string token = jwtTokenService.GenerateToken(userResp.Id, userResp.Email, DateTime.UtcNow.AddHours(1));
                try
                {
                    _ = sendRecoverPasswordEmailService.SendEmail(userResp.Email, token);
                }
                catch
                {
                    return new BaseResponse(null, "A error occurred!");
                }
            }

            return new BaseResponse("Email Sent.");
        }

        public async Task<BaseResponse> GenerateTokenAsync(ReqUserSession reqUserSession)
        {
            string? validateError = reqUserSession.Validate();

            if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(null, validateError);

            User? userResp = await userRepo.GetByEmailAndPasswordAsync(reqUserSession.Email, encryptionService.Encrypt(reqUserSession.Password));

            if (userResp is null) return new BaseResponse(null, "User/Password incorrect");

            string userJwt = jwtTokenService.GenerateToken(userResp.Id, userResp.Email, DateTime.UtcNow.AddDays(5));

            UserHistoric userHistoric = new() { UserHistoricTypeId = (int)UserHistoricTypeValues.SignIn, CreatedAt = DateTime.UtcNow, UserId = userResp.Id, User = userResp };

            await userHistoricRepo.ExecuteAddUserHistoric(userHistoric);

            ResToken resToken = new() { Token = userJwt };

            return new BaseResponse(resToken);
        }

        public async Task<BaseResponse> UpdatePasswordAsync(ReqRecoverPassword reqRecoverPassword, int uid)
        {
            try
            {
                string? validateError = reqRecoverPassword.Validate();

                if (string.IsNullOrEmpty(validateError) && reqRecoverPassword.Password != reqRecoverPassword.PasswordConfirmation)
                    validateError = "Invalid password Confirmation";

                if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(null, validateError);

                User? user = await userRepo.GetByIdAsync(uid);

                if (user != null)
                {
                    user.Password = encryptionService.Encrypt(reqRecoverPassword.Password);

                    await userRepo.UpdateAsync(user);

                    UserHistoric userHistoric = new() { UserHistoricTypeId = (int)UserHistoricTypeValues.PasswordChanged, CreatedAt = DateTime.UtcNow, UserId = user.Id, User = user };

                    await userHistoricRepo.ExecuteAddUserHistoric(userHistoric);

                    return new BaseResponse("Password Updated.");
                }
                else return new BaseResponse(null, "Invalid User");
            }
            catch { throw; }
        }

        protected async Task<string?> ValidateExistingUserAsync(User user)
        {
            User? userResp = await userRepo.GetByNameOrEmailAsync(user.Name, user.Email);

            if (userResp != null)
            {
                if (userResp.Name.Equals(user.Name))
                    return "User Name already exists.";

                if (userResp.Email.Equals(user.Email))
                    return "User Email already exists.";
            }

            return null;
        }
    }
}