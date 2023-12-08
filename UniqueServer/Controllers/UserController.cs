using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UserBLL;
using UserModels.Request.User;

namespace UniqueServer.Controllers
{
    [Route("[Controller]")]
    [ApiController]
    public class UserController(IUserBLL userBLL) : BaseController
    {
        [Route("")]
        [HttpPost]
        public async Task<IActionResult> SingUp(ReqUser reqUser) => BuildResponse(await userBLL.CreateUser(reqUser));

        [Route("Session")]
        [HttpPost]
        public async Task<IActionResult> SingIn(ReqUserSession reqUserSession) => BuildResponse(await userBLL.GenerateUserToken(reqUserSession));

        [Route("")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUser() => BuildResponse(await userBLL.GetUserById(Uid));

        [Route("RecoverPassword")]
        [HttpPost]
        public async Task<IActionResult> SendRecoverPasswordEmail(ReqUserEmail reqUserEmail) => BuildResponse(await userBLL.SendRecoverPasswordEmail(reqUserEmail));

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("RecoverPassword/{token}")]
        [HttpGet]
        public IActionResult RecoverPasswordBody(string token)
        {
            string html = System.IO.File.ReadAllText("StaticFiles/RecoverPassword/Index.html");

            html = html.Replace("{{token}}", token);

            return base.Content(html, "text/html");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("RecoverPassword/{token}")]
        [HttpPost]
        public IActionResult RecoverPassword(string token, [FromForm] ReqRecoverPassword reqRecoverPassword)
        {
            int? uid = UserManagementBLL.Functions.JwtFunctions.GetUidFromToken(token);

            if (uid == null) { NoContent(); }

            _ = userBLL.UpdatePassword(reqRecoverPassword, Convert.ToInt32(uid));

            string html = System.IO.File.ReadAllText("StaticFiles/RecoverPassword/PasswordUpdated.html");

            html = html.Replace("{{token}}", token);

            return base.Content(html, "text/html");
        }

    }
}
