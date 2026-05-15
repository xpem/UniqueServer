using FinancialService.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniqueServer.Controllers
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class FinancialController(ITransactionCategoryService transactionCategoryService) : BaseController
    {
        [Route("categories")]
        [HttpGet]
        public async Task<IActionResult> GetCategories(int uid, DateTime updatedAt)
        {
            //format 2023-06-10T21:53:28.331Z
            var categories = await transactionCategoryService.GetByUid(uid, updatedAt);
            return Ok(categories);
        }

        //[HttpGet("initBd")]
        //[HttpGet]
        //public IActionResult InitBd()
        //{
        //    financialInitDb.CreateCategoryInitialValues();
        //    return View();
        //}
    }
}
