using BaseModels;
using BaseModels.Configs;
using UserManagementModels.Response;
using UserManagementService.Functions;
using UserManagementModels.Request.User;
using UserManagementModels;
using UserManagementRepo;
using UserManagementService.Interfaces;
using Google.Apis.Auth;

namespace UserManagementService
{
    public class UserService(IUserRepo userRepo, IUserHistoricRepo userHistoricRepo,
        ISendRecoverPasswordEmailService sendRecoverPasswordEmailService, IEncryptionService encryptionService,
        IJwtTokenService jwtTokenService, GoogleAuthKeys googleAuthKeys) : IUserService
    {
        public async Task<BaseResp> CreateAsync(ReqUser reqUser)
        {
            string? validateError = reqUser.Validate();

            if (!string.IsNullOrEmpty(validateError)) return new BaseResp(ErrorCode.InvalidObject, validateError);

            User user = new() { Name = reqUser.Name, Email = reqUser.Email, Password = reqUser.Password, CreatedAt = DateTime.Now, IsGoogleAuth = false };

            string? existingUserMessage = await ValidateExistingUserAsync(user);
            if (existingUserMessage != null) { return new BaseResp(ErrorCode.TryCreateExistingUser, existingUserMessage); }

            if (user.Password != null)
                user.Password = encryptionService.Encrypt(user.Password);
            else throw new NullReferenceException("Password do usuario nulo");

            await userRepo.CreateAsync(user);

            ResUser? resUser;

            if (user?.Id is not null)
                resUser = new() { Id = user.Id, Name = user.Name, Email = user.Email, CreatedAt = user.CreatedAt };
            else throw new NullReferenceException("Id do usuário nulo");

            return new BaseResp(resUser);
        }

        public async Task<BaseResp> GoogleAuthAsync(string idToken)
        {
            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [googleAuthKeys.clientId]
                });
            }
            catch (InvalidJwtException)
            {
                return new BaseResp(ErrorCode.GoogleAuthNullEmailOrName, "Token do Google inválido");
            }

            string? name = payload.Name;
            string? email = payload.Email;

            if (name is null || email is null)
                return new BaseResp(ErrorCode.GoogleAuthNullEmailOrName, "Conta do google sem email ou nome");

            User? user = await userRepo.GetByEmailAsync(email);

            if (user is null)
            {
                user = new() { Name = name, Email = email, Password = null, CreatedAt = DateTime.Now, IsGoogleAuth = true };
                await userRepo.CreateAsync(user);
            }

            if (!user.IsGoogleAuth)
                return new BaseResp(errorCode: ErrorCode.UserEmailPasswordLoginType, "Email do usuário vinculado à acesso de email e senha");

            string userJwt = jwtTokenService.GenerateToken(user.Id, user.Email, DateTime.UtcNow.AddDays(15));

            string refreshToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(90);
            await userRepo.UpdateAsync(user);

            UserHistoric userHistoric = new() { UserHistoricTypeId = UserHistoricTypeValues.SignInGoogleAuth, CreatedAt = DateTime.UtcNow, UserId = user.Id, User = user };

            await userHistoricRepo.AddAsync(userHistoric);

            ResToken resToken = new() { Token = userJwt, RefreshToken = refreshToken };

            return new BaseResp(resToken);
        }

        public async Task<BaseResp> GetByIdAsync(int uid)
        {
            //todo - utilizar tmbm o email?
            User? userResp = await userRepo.GetByIdAsync(uid);

            if (userResp == null)
                return new BaseResp("User not found");

            return new BaseResp(new ResUser() { Id = userResp.Id, Name = userResp.Name, Email = userResp.Email, CreatedAt = userResp.CreatedAt });
        }

        public async Task<BaseResp> SendRecoverPasswordEmailAsync(ReqUserEmail reqUserEmail)
        {
            string? validateError = reqUserEmail.Validate();

            if (!string.IsNullOrEmpty(validateError)) return new BaseResp(ErrorCode.InvalidObject, validateError);

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
                    return new BaseResp(ErrorCode.SendEmailError, "Ocorreu um erro tentando enviar o email!");
                }
            }

            return new BaseResp("Email Sent.");
        }

        public async Task<BaseResp> GenerateTokenAsync(ReqUserSession reqUserSession)
        {
            string? validateError = reqUserSession.Validate();

            if (!string.IsNullOrEmpty(validateError)) return new BaseResp(ErrorCode.InvalidObject, validateError);

            User? userResp = await userRepo.GetByEmailAndPasswordAsync(reqUserSession.Email, encryptionService.Encrypt(reqUserSession.Password));

            if (userResp is null) return new BaseResp(ErrorCode.InvalidUserPasswordLogin, "User/Password incorrect");

            string userJwt = jwtTokenService.GenerateToken(userResp.Id, userResp.Email, DateTime.UtcNow.AddDays(15));

            string refreshToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            userResp.RefreshToken = refreshToken;
            userResp.RefreshTokenExpiry = DateTime.UtcNow.AddDays(90);
            await userRepo.UpdateAsync(userResp);

            UserHistoric userHistoric = new() { UserHistoricTypeId = UserHistoricTypeValues.SignIn, CreatedAt = DateTime.UtcNow, UserId = userResp.Id };

            await userHistoricRepo.AddAsync(userHistoric);

            ResToken resToken = new() { Token = userJwt, RefreshToken = refreshToken };

            return new BaseResp(resToken);
        }

        public async Task<BaseResp> RefreshTokenAsync(ReqRefreshToken reqRefreshToken)
        {
            string? validateError = reqRefreshToken.Validate();

            if (!string.IsNullOrEmpty(validateError)) return new BaseResp(ErrorCode.InvalidObject, validateError);

            User? user = await userRepo.GetByRefreshTokenAsync(reqRefreshToken.RefreshToken);

            if (user is null || user.RefreshTokenExpiry is null || user.RefreshTokenExpiry < DateTime.UtcNow)
                return new BaseResp(ErrorCode.InvalidUserPasswordLogin, "Invalid or expired refresh token");

            string userJwt = jwtTokenService.GenerateToken(user.Id, user.Email, DateTime.UtcNow.AddDays(15));

            string newRefreshToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(90);
            await userRepo.UpdateAsync(user);

            ResToken resToken = new() { Token = userJwt, RefreshToken = newRefreshToken };

            return new BaseResp(resToken);
        }

        public async Task<BaseResp> UpdatePasswordAsync(ReqRecoverPassword reqRecoverPassword, int uid)
        {
            try
            {
                string? validateError = reqRecoverPassword.Validate();

                if (string.IsNullOrEmpty(validateError) && reqRecoverPassword.Password != reqRecoverPassword.PasswordConfirmation)
                    validateError = "Invalid password Confirmation";

                if (!string.IsNullOrEmpty(validateError)) return new BaseResp(ErrorCode.InvalidPasswordConfirmation, validateError);

                User? user = await userRepo.GetByIdAsync(uid);

                if (user != null)
                {
                    user.Password = encryptionService.Encrypt(reqRecoverPassword.Password);

                    await userRepo.UpdateAsync(user);

                    UserHistoric userHistoric = new() { UserHistoricTypeId = UserHistoricTypeValues.PasswordChanged, CreatedAt = DateTime.UtcNow, UserId = user.Id };

                    await userHistoricRepo.AddAsync(userHistoric);

                    return new BaseResp("Password Updated.");
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