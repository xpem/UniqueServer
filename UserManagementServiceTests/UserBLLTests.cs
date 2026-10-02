using BaseModels.Configs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UserManagementService.Model.Request.User;
using UserManagementService.Model.Response;
using UserManagementService.Model;
using UserManagementService.Repo;
using UserManagementService.Service;

namespace UserManagementRepoTests
{
    [TestClass()]
    public class UserBLLTests
    {
        [TestMethod()]
        public async Task GenerateUserTokenTest()
        {
            Mock<IUserRepo> userDAL = new();
            Mock<IUserHistoricRepo> userHistoricDAL = new();
            Mock<ISendRecoverPasswordEmailService> sendRecoverPasswordEmail = new();
            Mock<IEncryptionService> encryptionService = new();
            Mock<IJwtTokenService> jwtTokenService = new();
            Mock<IPasswordHashService> passwordHashService = new();

            string encryptedtoken = "test";
            ReqUserSession reqUserSession = new()
            {
                Email = "emanuel_teste@email.com",
                Password = "121212"
            };

            User userResp = new()
            {
                CreatedAt = DateTime.Now,
                Email = "emanuel_teste@email.com",
                Name = "emanuel",
                Password = "hashed-121212",
                PasswordAlgo = PasswordAlgo.Pbkdf2,
                Id = 1,
                IsGoogleAuth = false,
            };

            userDAL.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(userResp);
            userDAL.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(1);
            userHistoricDAL.Setup(x => x.AddAsync(It.IsAny<UserHistoric>())).ReturnsAsync(1);
            passwordHashService.Setup(x => x.Verify("121212", userResp.Password)).Returns(true);
            jwtTokenService.Setup(x => x.GenerateToken(userResp.Id, userResp.Email, It.IsAny<DateTime>())).Returns(encryptedtoken);

            UserService userService = new(userDAL.Object, userHistoricDAL.Object, sendRecoverPasswordEmail.Object, encryptionService.Object, jwtTokenService.Object, new GoogleAuthKeys("test-client-id","secret","url"), passwordHashService.Object);

            var resp = await userService.GenerateTokenAsync(reqUserSession);

            if (resp != null && resp.Content != null && resp.Content is ResToken)
            {
                var content = resp.Content as ResToken;

                Assert.AreEqual(content?.Token, encryptedtoken);
                return;
            }

            Assert.Fail();
        }
    }
}