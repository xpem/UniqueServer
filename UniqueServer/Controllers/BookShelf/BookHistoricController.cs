using BookshelfBLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniqueServer.Controllers.BookShelf
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class BookHistoricController(IBookHistoricBLL bookHistoricBLL) : BaseController
    {
        [Route("bycreatedat/{createdAt}")]
        [HttpGet]
        public IActionResult GetBookHistoricByUpdatedAt(string createdAt)
        {
            //format 2023-06-10T21:53:28.331Z
            return BuildResponse(bookHistoricBLL.GetByBookIdOrCreatedAt(null, DateTime.Parse(createdAt), Uid));
        }

        [Route("bybookid/{bookId}")]
        [HttpGet]
        public IActionResult GetBookHistoricByUpdatedAt(int bookId) => BuildResponse(bookHistoricBLL.GetByBookIdOrCreatedAt(bookId, null, Uid));
    }
}
