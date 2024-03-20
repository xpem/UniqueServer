using BaseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.IdentityModel.Tokens;
using UserBLL;
using UserModels.Request.User;

namespace UniqueServer.Controllers
{
    [Route("[Controller]")]
    [ApiController]
    public class UserController(IUserBLL userBLL, IHostEnvironment hostingEnvironment) : BaseController
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
            string html = System.IO.File.ReadAllText(Path.Combine(hostingEnvironment.ContentRootPath, "StaticFiles", "RecoverPassword", "Index.html"));

            html = html.Replace("{{token}}", token);

            return base.Content(html, "text/html");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("RecoverPassword/{token}")]
        [HttpPost]
        public async Task<IActionResult> RecoverPassword(string token, [FromForm] ReqRecoverPassword reqRecoverPassword)
        {
            string html = System.IO.File.ReadAllText(Path.Combine(hostingEnvironment.ContentRootPath, "StaticFiles", "RecoverPassword", "PasswordUpdated.html"));

            try
            {
                int? uid = UserManagementBLL.Functions.JwtFunctions.GetUidFromToken(token);

                html = html.Replace("{{token}}", token);

                if (uid == null)
                    html = html.Replace("{{ReturnMessage}}", "User Not Found");
                else
                {
                    var bLLResponse = await userBLL.UpdatePassword(reqRecoverPassword, Convert.ToInt32(uid));

                    if (bLLResponse.Success)
                        html = html.Replace("{{ReturnMessage}}", bLLResponse.Content?.ToString());
                    else html = html.Replace("{{ReturnMessage}}", bLLResponse.Error?.Message?.ToString());
                }
            }
            catch (SecurityTokenExpiredException)
            {
                html = html.Replace("{{ReturnMessage}}", "Email link expired.");
            }

            return base.Content(html, "text/html");
        }

    }
}
