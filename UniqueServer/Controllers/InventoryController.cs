using BookshelfBLL;
using BookshelfModels.Request;
using InventoryBLL;
using InventoryModels.Req;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniqueServer.Controllers
{
    [Route("[Controller]")]
    [ApiController]
    public class InventoryController(ICategoryBLL categoryBLL) : BaseController
    {
        [Route("")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CreateCategory()
        {
            ReqCategory reqCategory = new() { Name = string.Empty, Color = string.Empty };

            await categoryBLL.CreateCategory(reqCategory);
            //int? uid = RecoverUidSession();

            //return uid != null ? BuildResponse(await bookBLL.CreateBook(book, Convert.ToInt32(uid))) : NoContent();

            return NoContent();
        }

    }
}
