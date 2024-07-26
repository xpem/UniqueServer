using BaseModels;
using UserBLL.Functions;
using UserManagementBLL.Functions;
using UserModels;
using UserModels.Request.User;
using UserModels.Response;

namespace UserBLL
{
    public class UserBLL(UserManagementDAL.IUserDAL userDAL, UserManagementDAL.IUserHistoricDAL userHistoricDAL,
        ISendRecoverPasswordEmailService sendRecoverPasswordEmail, IEncryptionService encryptionService,
        IJwtTokenService jwtTokenService) : IUserBLL
    {
        public async Task<BaseResponse> CreateUser(ReqUser reqUser)
        {
            string? validateError = reqUser.Validate();

            if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(null, validateError);

            User user = new() { Name = reqUser.Name, Email = reqUser.Email, Password = reqUser.Password, CreatedAt = DateTime.Now };

            string? existingUserMessage = await ValidateExistingUser(user);
            if (existingUserMessage != null) { return new BaseResponse(null, existingUserMessage); }

            if (user.Password != null)
                user.Password = encryptionService.Encrypt(user.Password);
            else throw new NullReferenceException("Password do usuario nulo");

            await userDAL.ExecuteCreateUserAsync(user);

            ResUser? resUser;

            if (user?.Id is not null)
                resUser = new() { Id = user.Id, Name = user.Name, Email = user.Email, CreatedAt = user.CreatedAt };
            else throw new NullReferenceException("Id do usuário nulo");

            return new BaseResponse(resUser);
        }

        public async Task<BaseResponse> GetUserById(int uid)
        {
            //todo - utilizar tmbm o email?
            User? userResp = await userDAL.GetUserByIdAsync(uid);
            if (userResp == null)
                return new BaseResponse(null, "User not found");

            return new BaseResponse(new ResUser() { Id = userResp.Id, Name = userResp.Name, Email = userResp.Email, CreatedAt = userResp.CreatedAt });
        }

        public async Task<BaseResponse> SendRecoverPasswordEmail(ReqUserEmail reqUserEmail)
        {
            string? validateError = reqUserEmail.Validate();

            if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(null, validateError);

            User? userResp = await userDAL.GetUserByEmailAsync(reqUserEmail.Email);

            if (userResp != null)
            {
                string token = jwtTokenService.GenerateToken(userResp.Id, userResp.Email, DateTime.UtcNow.AddHours(1));
                try
                {
                    _ = sendRecoverPasswordEmail.SendEmail(userResp.Email, token);
                }
                catch
                {
                    return new BaseResponse(null, "A error occurred!");
                }
            }

            return new BaseResponse("Email Sent.");
        }

        public async Task<BaseResponse> GenerateUserToken(ReqUserSession reqUserSession)
        {
            string? validateError = reqUserSession.Validate();

            if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(null, validateError);

            User? userResp = await userDAL.GetUserByEmailAndPassword(reqUserSession.Email, encryptionService.Encrypt(reqUserSession.Password));

            if (userResp is null) return new BaseResponse(null, "User/Password incorrect");

            string userJwt = jwtTokenService.GenerateToken(userResp.Id, userResp.Email, DateTime.UtcNow.AddDays(5));

            UserHistoric userHistoric = new() { UserHistoricTypeId = (int)UserHistoricTypeValues.SignIn, CreatedAt = DateTime.UtcNow, UserId = userResp.Id, User = userResp };

            await userHistoricDAL.ExecuteAddUserHistoric(userHistoric);

            return new BaseResponse(new { Token = userJwt });
        }

        public async Task<BaseResponse> UpdatePassword(ReqRecoverPassword reqRecoverPassword, int uid)
        {
            try
            {
                string? validateError = reqRecoverPassword.Validate();

                if (string.IsNullOrEmpty(validateError) && reqRecoverPassword.Password != reqRecoverPassword.PasswordConfirmation)
                    validateError = "Invalid password Confirmation";

                if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(null, validateError);

                User? user = await userDAL.GetUserByIdAsync(uid);

                if (user != null)
                {
                    user.Password = encryptionService.Encrypt(reqRecoverPassword.Password);

                    await userDAL.ExecuteUpdateUserAsync(user);

                    UserHistoric userHistoric = new() { UserHistoricTypeId = (int)UserHistoricTypeValues.PasswordChanged, CreatedAt = DateTime.UtcNow, UserId = user.Id, User = user };

                    await userHistoricDAL.ExecuteAddUserHistoric(userHistoric);

                    return new BaseResponse("Password Updated.");
                }
                else return new BaseResponse(null, "Invalid User");
            }
            catch (Exception ex) { throw ex; }
        }

        protected async Task<string?> ValidateExistingUser(User user)
        {
            User? userResp = await userDAL.GetUserByNameOrEmailAsync(user.Name, user.Email);

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