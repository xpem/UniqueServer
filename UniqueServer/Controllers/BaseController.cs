using BaseModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace UniqueServer.Controllers
{
    public class BaseController : Controller
    {
        protected IActionResult BuildResponse(BLLResponse bllResp) => ((!string.IsNullOrEmpty(bllResp.Error?.Error)) ? BadRequest(bllResp.Error) : Ok(bllResp.Content));

        protected int? RecoverUidSession()
        {
            string? uid = null;

            if (HttpContext.User.Identity is ClaimsIdentity identity)
                uid = identity.Claims.FirstOrDefault(x => x.Type == "uid")?.Value;

            return uid != null ? Convert.ToInt32(uid) : null;
        }
    }
}
