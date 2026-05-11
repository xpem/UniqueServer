using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniqueServer.Controllers
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class FinancialController : BaseController
    {
        [Route("categories")]
        [HttpGet]
        public IActionResult GetCategories()
        {
            return View();
        }
    }
}
