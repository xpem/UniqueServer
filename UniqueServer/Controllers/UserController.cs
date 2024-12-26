using BaseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UserManagementService.Functions;
using UserManagementModels.Request.User;
using UserManagementService;

namespace UniqueServer.Controllers
{
    [Route("[Controller]")]
    [ApiController]
    public class UserController(IUserService userService, IHostEnvironment hostingEnvironment, IJwtTokenService jwtTokenService, IUserDataDeleteService userDataDeleteService) : BaseController
    {
        [Route("")]
        [HttpPost]
        public async Task<IActionResult> SignUp(ReqUser reqUser) => BuildResponse(await userService.CreateAsync(reqUser));

        [Route("Session")]
        [HttpPost]
        public async Task<IActionResult> SignIn(ReqUserSession reqUserSession) => BuildResponse(await userService.GenerateTokenAsync(reqUserSession));

        [Route("")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUser() => BuildResponse(await userService.GetByIdAsync(Uid));

        [Route("RecoverPassword")]
        [HttpPost]
        public async Task<IActionResult> SendRecoverPasswordEmail(ReqUserEmail reqUserEmail) => BuildResponse(await userService.SendRecoverPasswordEmailAsync(reqUserEmail));

        [Route("UserDataExclusion")]
        [HttpPost]
        public async Task<IActionResult> UserDataExclusion([FromForm] ReqUserDataExclusion reqUserDataExclusion)
        {
            var resp = await userDataDeleteService.DeleteAsync(reqUserDataExclusion);

            string html = System.IO.File.ReadAllText(Path.Combine(hostingEnvironment.ContentRootPath, "StaticFiles", "UserDataExclusion", "UserDataExclusionConfirmation.html"));

            if (resp.Success)
            {
                html = html.Replace("{{TypeMessage}}", "success");
                html = html.Replace("{{ReturnMessage}}", "Usuário excluído com sucesso");
            }
            else
            {
                html = html.Replace("{{TypeMessage}}", "warning");
                html = html.Replace("{{ReturnMessage}}", resp.Error?.Message);
            }

            return base.Content(html, "text/html");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("PrivacyPolicy")]
        [HttpGet]
        public IActionResult PrivacyPolicy()
        {
            string html = System.IO.File.ReadAllText(Path.Combine(hostingEnvironment.ContentRootPath, "StaticFiles", "PrivacyPolicy", "Index.html"));

            return base.Content(html, "text/html");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("UserDataExclusion")]
        [HttpGet]
        public IActionResult UserDataExclusionBody()
        {
            string html = System.IO.File.ReadAllText(Path.Combine(hostingEnvironment.ContentRootPath, "StaticFiles", "UserDataExclusion", "Index.html"));

            return base.Content(html, "text/html");
        }

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
                int? uid = jwtTokenService.GetUidFromToken(token);

                html = html.Replace("{{token}}", token);

                if (uid == null)
                    html = html.Replace("{{ReturnMessage}}", "User Not Found");
                else
                {
                    BaseResponse bLLResponse = await userService.UpdatePasswordAsync(reqRecoverPassword, Convert.ToInt32(uid));

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
