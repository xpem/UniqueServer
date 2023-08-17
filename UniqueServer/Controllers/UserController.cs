using BaseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserBLL;
using UserModels.Request.User;

namespace UniqueServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        readonly IUserBLL userBLL;

        public UserController(IUserBLL userBLL)
        {
            this.userBLL = userBLL;
        }

        private IActionResult BuildResponse(BLLResponse bllResp) => ((!string.IsNullOrEmpty(bllResp.Error?.Error)) ? BadRequest(bllResp.Error) : Ok(bllResp.Content));

        private int? RecoverUidSession()
        {
            string? uid = null;

            if (HttpContext.User.Identity is ClaimsIdentity identity)
                uid = identity.Claims.FirstOrDefault(x => x.Type == "uid")?.Value;

            return uid != null ? Convert.ToInt32(uid) : null;
        }

        [Route("")]
        [HttpPost]
        public async Task<IActionResult> SingUp(ReqUser reqUser) => BuildResponse(await userBLL.CreateUser(reqUser));

        [Route("Session")]
        [HttpPost]
        public async Task<IActionResult> SingIn(ReqUserSession reqUserSession) => BuildResponse(await userBLL.GenerateUserToken(reqUserSession));

        [Route("")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUser()
        {
            int? uid = RecoverUidSession();
            return uid != null ? BuildResponse(await userBLL.GetUserById(Convert.ToInt32(uid))) : NoContent();
        }

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
            int? uid = BaseBLL.Functions.JwtFunctions.GetUidFromToken(token);

            if (uid == null) { NoContent(); }

            _ = userBLL.UpdatePassword(reqRecoverPassword, Convert.ToInt32(uid));

            string html = System.IO.File.ReadAllText("StaticFiles/RecoverPassword/PasswordUpdated.html");

            html = html.Replace("{{token}}", token);

            return base.Content(html, "text/html");
        }

    }
}
