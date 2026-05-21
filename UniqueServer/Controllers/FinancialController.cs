using FinancialService.Model.Req;
using FinancialService.Model.Res;
using FinancialService.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniqueServer.Controllers
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class FinancialController(ITransactionCategoryService transactionCategoryService,ITransactionService transactionService) : BaseController
    {
        [Route("categories")]
        [HttpGet]
        public async Task<IActionResult> GetCategories(DateTime? updatedAt)
        {
            updatedAt ??= DateTime.MinValue;

            //format 2023-06-10T21:53:28.331Z
            List<TransactionCategoryRes> categories = await transactionCategoryService.GetByUid(Uid, updatedAt.Value);
            return Ok(categories);
        }

        [Route("transaction")]
        [HttpPost]
        public async Task<IActionResult> AddTransaction([FromBody] TransactionReq req)
        {
            string result = await transactionService.AddAsync(req, Uid);
            return Ok(result);
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
