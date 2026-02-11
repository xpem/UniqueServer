using BaseModels;
using UserManagementModels.Response;
using UserManagementService.Functions;
using UserManagementModels.Request.User;
using UserManagementModels;
using UserManagementRepo;
using UserManagementService.Interfaces;

namespace UserManagementService
{
    public class UserService(IUserRepo userRepo, IUserHistoricRepo userHistoricRepo,
        ISendRecoverPasswordEmailService sendRecoverPasswordEmailService, IEncryptionService encryptionService,
        IJwtTokenService jwtTokenService) : IUserService
    {
        public async Task<BaseResponse> CreateAsync(ReqUser reqUser)
        {
            string? validateError = reqUser.Validate();

            if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(ErrorCode.InvalidObject, validateError);

            User user = new() { Name = reqUser.Name, Email = reqUser.Email, Password = reqUser.Password, CreatedAt = DateTime.Now, IsGoogleAuth = false };

            string? existingUserMessage = await ValidateExistingUserAsync(user);
            if (existingUserMessage != null) { return new BaseResponse(ErrorCode.TryCreateExistingUser, existingUserMessage); }

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

        public async Task<BaseResponse> GoogleAuthAsync(string? name, string? email)
        {
            if (name is null || email is null)
                return new BaseResponse(ErrorCode.GoogleAuthNullEmailOrName, "Conta do google sem email ou nome");

            User? user = await userRepo.GetByEmailAsync(email);

            if (user is null)
            {
                user = new() { Name = name, Email = email, Password = null, CreatedAt = DateTime.Now, IsGoogleAuth = true };
                await userRepo.CreateAsync(user);
            }

            if (!user.IsGoogleAuth)
                return new BaseResponse(errorCode: ErrorCode.UserEmailPasswordLoginType, "Email do usuário vinculado à acesso de email e senha");

            string userJwt = jwtTokenService.GenerateToken(user.Id, user.Email, DateTime.UtcNow.AddDays(15));

            UserHistoric userHistoric = new() { UserHistoricTypeId = UserHistoricTypeValues.SignInGoogleAuth, CreatedAt = DateTime.UtcNow, UserId = user.Id, User = user };

            await userHistoricRepo.AddAsync(userHistoric);

            ResToken resToken = new() { Token = userJwt };

            return new BaseResponse(resToken);
        }

        public async Task<BaseResponse> GetByIdAsync(int uid)
        {
            //todo - utilizar tmbm o email?
            User? userResp = await userRepo.GetByIdAsync(uid);

            if (userResp == null)
                return new BaseResponse("User not found");

            return new BaseResponse(new ResUser() { Id = userResp.Id, Name = userResp.Name, Email = userResp.Email, CreatedAt = userResp.CreatedAt });
        }

        public async Task<BaseResponse> SendRecoverPasswordEmailAsync(ReqUserEmail reqUserEmail)
        {
            string? validateError = reqUserEmail.Validate();

            if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(ErrorCode.InvalidObject, validateError);

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
                    //gravar um log de erro
                    return new BaseResponse(ErrorCode.SendEmailError, "Ocorreu um erro tentando enviar o email!");
                }
            }

            return new BaseResponse("Email Sent.");
        }

        public async Task<BaseResponse> GenerateTokenAsync(ReqUserSession reqUserSession)
        {
            string? validateError = reqUserSession.Validate();

            if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(ErrorCode.InvalidObject, validateError);

            User? userResp = await userRepo.GetByEmailAndPasswordAsync(reqUserSession.Email, encryptionService.Encrypt(reqUserSession.Password));

            if (userResp is null) return new BaseResponse(ErrorCode.InvalidUserPasswordLogin, "User/Password incorrect");

            string userJwt = jwtTokenService.GenerateToken(userResp.Id, userResp.Email, DateTime.UtcNow.AddDays(15));

            UserHistoric userHistoric = new() { UserHistoricTypeId = UserHistoricTypeValues.SignIn, CreatedAt = DateTime.UtcNow, UserId = userResp.Id, User = userResp };

            await userHistoricRepo.AddAsync(userHistoric);

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

                if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(ErrorCode.InvalidPasswordConfirmation, validateError);

                User? user = await userRepo.GetByIdAsync(uid);

                if (user != null)
                {
                    user.Password = encryptionService.Encrypt(reqRecoverPassword.Password);

                    await userRepo.UpdateAsync(user);

                    UserHistoric userHistoric = new() { UserHistoricTypeId = UserHistoricTypeValues.PasswordChanged, CreatedAt = DateTime.UtcNow, UserId = user.Id, User = user };

                    await userHistoricRepo.AddAsync(userHistoric);

                    return new BaseResponse("Password Updated.");
                }
                else throw new Exception("Invalid User, uid:" + uid);
            }
            catch { throw; }
        }

        protected async Task<string?> ValidateExistingUserAsync(User user)
        {
            User? userResp = await userRepo.GetByEmailAsync(user.Email);

            if (userResp != null)
            {
                if (userResp.Email.Equals(user.Email))
                    return "User Email already exists.";
            }

            return null;
        }
    }
}