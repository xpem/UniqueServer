﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using UserBLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagementDAL;
using Moq;
using UserBLL.Functions;
using UserManagementBLL.Functions;
using UserModels.Request.User;
using UserModels;
using BaseModels;
using System.Text.Json.Nodes;
using UserManagementModels.Response;

namespace UserBLL.Tests
{
    [TestClass()]
    public class UserBLLTests
    {
        [TestMethod()]
        public async Task GenerateUserTokenTest()
        {
            Mock<IUserDAL> userDAL = new();
            Mock<IUserHistoricDAL> userHistoricDAL = new();
            Mock<ISendRecoverPasswordEmailService> sendRecoverPasswordEmail = new();
            Mock<IEncryptionService> encryptionService = new();
            Mock<IJwtTokenService> jwtTokenService = new();


            string encryptedPassword = "test";
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
                Password = "121212",
                Id = 1,
            };

            userDAL.Setup(x => x.GetUserByEmailAndPassword(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(userResp);
            userHistoricDAL.Setup(x => x.ExecuteAddUserHistoric(It.IsAny<UserHistoric>())).ReturnsAsync(1);
            encryptionService.Setup(x => x.Encrypt(It.IsAny<string>())).Returns(encryptedPassword);
            jwtTokenService.Setup(x => x.GenerateToken(userResp.Id, userResp.Email, It.IsAny<DateTime>())).Returns(encryptedtoken);

            UserService userService = new(userDAL.Object, userHistoricDAL.Object, sendRecoverPasswordEmail.Object,
                encryptionService.Object, jwtTokenService.Object);

            var resp = await userService.GenerateUserToken(reqUserSession);

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