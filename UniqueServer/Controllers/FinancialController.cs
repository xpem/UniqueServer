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
    public class FinancialController(ITransactionCategoryService transactionCategoryService, ITransactionService transactionService, IAccountService accountService, IRecurringRuleService recurringRuleService) : BaseController
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

        [HttpPost]
        [Route("adjustAccountBalance")]
        public async Task<IActionResult> AdjustAccountBalance([FromBody] AdjustAccountBalanceReq req)
        {
            var result = await accountService.AdjustAccountBalanceAsync(req, Uid);
            return Ok(result);
        }

        [HttpGet]
        [Route("account")]
        public async Task<IActionResult> GetAccount(DateTime? updatedAt)
        {
            updatedAt ??= DateTime.MinValue;

            var result = await accountService.GetAsync(Uid, updatedAt.Value);
            if (result is null) return NoContent();
            return Ok(result);
        }

        [HttpGet]
        [Route("accounts")]
        public async Task<IActionResult> GetAccounts(DateTime? updatedAt)
        {
            updatedAt ??= DateTime.MinValue;

            var result = await accountService.GetUpdatedAfterAsync(Uid, updatedAt.Value);
            if (result.Count == 0) return NoContent();
            return Ok(result);
        }

        [HttpPost]
        [Route("account")]
        public async Task<IActionResult> CreateAccount([FromBody] AccountReq req)
        {
            try
            {
                var result = await accountService.CreateAsync(req, Uid);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("account/{id}")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] AccountReq req)
        {
            try
            {
                var result = await accountService.UpdateAsync(id, req, Uid);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("category")]
        public async Task<IActionResult> AddCategory([FromBody] TransactionCategoryReq req)
        {
            var validationError = req.Validate();
            if (validationError is not null) return BadRequest(validationError);

            var result = await transactionCategoryService.UpsertAsync(req, Uid);
            return Ok(result);
        }

        [Route("transaction")]
        [HttpPost]
        public async Task<IActionResult> AddTransaction([FromBody] TransactionReq req)
        {
            var result = await transactionService.AddAsync(req, Uid);
            return Ok(result);
        }

        [Route("transaction/{id}")]
        [HttpPut]
        public async Task<IActionResult> UpdateTransaction(int id, [FromBody] TransactionReq req)
        {
            await transactionService.UpdateAsync(id, req, Uid);
            return NoContent();
        }

        [Route("transaction")]
        [HttpGet]
        public async Task<IActionResult> GetTransactions(DateTime? updatedAt)
        {
            updatedAt ??= DateTime.MinValue;

            var result = await transactionService.GetByUpdatedAtAsync(Uid, updatedAt.Value);
            if (result.Count == 0) return NoContent();
            return Ok(result);
        }

        [Route("recurringrule")]
        [HttpPost]
        public async Task<IActionResult> AddRecurringRule([FromBody] RecurringRuleReq req)
        {
            var result = await recurringRuleService.AddOrUpdateAsync(req, Uid);
            return Ok(result);
        }

        [Route("recurringrule")]
        [HttpGet]
        public async Task<IActionResult> GetRecurringRules(DateTime? updatedAt)
        {
            updatedAt ??= DateTime.MinValue;

            var result = await recurringRuleService.GetByUpdatedAtAsync(Uid, updatedAt.Value);
            if (result.Count == 0) return NoContent();
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
