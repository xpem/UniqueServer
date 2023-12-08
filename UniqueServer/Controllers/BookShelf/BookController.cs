using BookshelfBLL;
using BookshelfModels.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniqueServer.Controllers.BookShelf
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class BookController(IBookBLL bookBLL) : BaseController
    {
        [Route("")]
        [HttpPost]
        public async Task<IActionResult> CreateBook(ReqBook book) => BuildResponse(await bookBLL.CreateBook(book, Uid));

        [Route("{id}")]
        [HttpPut]
        public async Task<IActionResult> UpdateBook(ReqBook book, int id) => BuildResponse(await bookBLL.UpdateBook(book, id, Uid));


        [Route("byupdatedat/{UpdatedAt}")]
        [HttpGet]
        public IActionResult GetBookByUpdatedAt(string updatedAt)
        {
            //format 2023-06-10T21:53:28.331Z
            return BuildResponse(bookBLL.GetByUpdatedAt(DateTime.Parse(updatedAt), Uid));
        }

        [Route("inactivate/{id}")]
        [HttpDelete]
        public async Task<IActionResult> InactivateBook(int id) => BuildResponse(await bookBLL.InactivateBook(id, Uid));

    }
}

