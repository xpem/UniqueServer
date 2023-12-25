using BookshelfBLL;
using BookshelfModels.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniqueServer.Controllers.BookShelf
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class BookshelfController(IBookBLL bookBLL, IBookHistoricBLL bookHistoricBLL) : BaseController
    {
        [Route("book")]
        [HttpPost]
        public async Task<IActionResult> CreateBook(ReqBook book) => BuildResponse(await bookBLL.CreateBook(book, Uid));

        [Route("book/{id}")]
        [HttpPut]
        public async Task<IActionResult> UpdateBook(ReqBook book, int id) => BuildResponse(await bookBLL.UpdateBook(book, id, Uid));


        [Route("book/byupdatedat/{UpdatedAt}")]
        [HttpGet]
        public IActionResult GetBookByUpdatedAt(string updatedAt)
        {
            //format 2023-06-10T21:53:28.331Z
            return BuildResponse(bookBLL.GetByUpdatedAt(DateTime.Parse(updatedAt), Uid));
        }

        [Route("book/inactivate/{id}")]
        [HttpDelete]
        public async Task<IActionResult> InactivateBook(int id) => BuildResponse(await bookBLL.InactivateBook(id, Uid));

        [Route("book/historic/bycreatedat/{createdAt}")]
        [HttpGet]
        public IActionResult GetBookHistoricByUpdatedAt(string createdAt)
        {
            //format 2023-06-10T21:53:28.331Z
            return BuildResponse(bookHistoricBLL.GetByBookIdOrCreatedAt(null, DateTime.Parse(createdAt), Uid));
        }

        [Route("book/historic/bybookid/{bookId}")]
        [HttpGet]
        public IActionResult GetBookHistoricByUpdatedAt(int bookId) => BuildResponse(bookHistoricBLL.GetByBookIdOrCreatedAt(bookId, null, Uid));

    }
}

