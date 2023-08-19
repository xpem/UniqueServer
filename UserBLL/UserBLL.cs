using BaseBLL.Functions;
using BaseModels;
using Microsoft.EntityFrameworkCore;
using UserManagementDAL;
using UserModels;
using UserModels.Request.User;
using UserModels.Response;

namespace UserBLL
{
    public class UserBLL : IUserBLL
    {
        readonly UserManagementDbContext AppDbContext;

        public UserBLL(UserManagementDbContext appdbContext) { AppDbContext = appdbContext; }

        public async Task<BLLResponse> CreateUser(ReqUser reqUser)
        {
            string? validateError = reqUser.Validate();

            if (!string.IsNullOrEmpty(validateError)) return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = validateError } }; ;

            User user = new() { Name = reqUser.Name, Email = reqUser.Email, Password = reqUser.Password, CreatedAt = DateTime.Now };

            string? existingUserMessage = await ValidateExistingUser(user);
            if (existingUserMessage != null) { return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = existingUserMessage } }; }

            if (user.Password != null)
                user.Password = Encryption.Encrypt(user.Password);
            else throw new NullReferenceException("Password do usuario nulo");

            await AppDbContext.User.AddAsync(user);

            await AppDbContext.SaveChangesAsync();

            ResUser? resUser;

            if (user?.Id is not null)
                resUser = new() { Id = user.Id, Name = user.Name, Email = user.Email, CreatedAt = user.CreatedAt };
            else throw new NullReferenceException("Id do usuário nulo");

            return new BLLResponse() { Content = resUser, Error = null };
        }

        public async Task<BLLResponse> GetUserById(int uid)
        {
            //todo - utilizar tmbm o email?
            User? userResp = await AppDbContext.User.FirstOrDefaultAsync(x => x.Id.Equals(uid));
            if (userResp == null)
                return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = "User not found" } };

            return new BLLResponse() { Content = new ResUser() { Id = userResp.Id, Name = userResp.Name, Email = userResp.Email, CreatedAt = userResp.CreatedAt }, Error = null };
        }

        public async Task<BLLResponse> SendRecoverPasswordEmail(ReqUserEmail reqUserEmail)
        {
            string? validateError = reqUserEmail.Validate();

            if (!string.IsNullOrEmpty(validateError)) return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = validateError } };

            User? userResp = await AppDbContext.User.FirstOrDefaultAsync(x => x.Email.Equals(reqUserEmail.Email));

            if (userResp != null)
            {
                string token = JwtFunctions.GenerateToken(userResp.Id, userResp.Email, DateTime.UtcNow.AddHours(1));

                _ = Functions.SendRecoverPasswordEmail.SendEmail(userResp.Email, token);
            }

            return new BLLResponse() { Content = new { Mensagem = "Email Sent." }, Error = null };

        }

        public async Task<BLLResponse> GenerateUserToken(ReqUserSession reqUserSession)
        {
            string? validateError = reqUserSession.Validate();

            if (!string.IsNullOrEmpty(validateError)) return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = validateError } };

            User? userResp = await AppDbContext.User.FirstOrDefaultAsync(x => x.Email == reqUserSession.Email && x.Password == Encryption.Encrypt(reqUserSession.Password));

            if (userResp is null) return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = "User/Password incorrect" } };

            string userJwt = JwtFunctions.GenerateToken(userResp.Id, userResp.Email, DateTime.UtcNow.AddDays(5));

            UserHistoric userHistoric = new() { UserHistoricTypeId = (int)UserHistoricTypeValues.SignIn, CreatedAt = DateTime.UtcNow, UserId = userResp.Id, User = userResp };

            await AppDbContext.UserHistoric.AddAsync(userHistoric);

            await AppDbContext.SaveChangesAsync();

            return new BLLResponse() { Content = new { Token = userJwt }, Error = null };
        }

        public async Task<BLLResponse> UpdatePassword(ReqRecoverPassword reqRecoverPassword, int uid)
        {
            string? validateError = reqRecoverPassword.Validate();

            if (string.IsNullOrEmpty(validateError) && reqRecoverPassword.Password != reqRecoverPassword.PasswordConfirmation)
                validateError = "Invalid password Confirmation";

            if (!string.IsNullOrEmpty(validateError)) return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = validateError } };

            var user = await AppDbContext.User.FirstOrDefaultAsync(x => x.Id == uid);

            if (user != null)
            {
                user.Password = Encryption.Encrypt(reqRecoverPassword.Password);

                AppDbContext.User.Update(user);

                UserHistoric userHistoric = new() { UserHistoricTypeId = (int)UserHistoricTypeValues.PasswordChanged, CreatedAt = DateTime.UtcNow, UserId = user.Id, User = user };

                await AppDbContext.UserHistoric.AddAsync(userHistoric);

                await AppDbContext.SaveChangesAsync();

                return new BLLResponse() { Content = "Password Updated.", Error = new ErrorMessage() { Error = null } };
            }
            else return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = "Invalid User" } };
        }

        protected async Task<string?> ValidateExistingUser(User user)
        {
            User? userResp = await AppDbContext.User.FirstOrDefaultAsync(x => x.Name.Equals(user.Name) || x.Email.Equals(user.Email));

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