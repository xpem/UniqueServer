using BookshelfBLL;
using BookshelfModels.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniqueServer.Controllers
{

    [Route("[Controller]")]
    [ApiController]
    public class BookController : BaseController
    {
        private readonly IBookBLL bookBLL;

        public BookController(IBookBLL bookBLL)
        {
            this.bookBLL = bookBLL;
        }

        [Route("")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateBook(ReqBook book)
        {
            int? uid = RecoverUidSession();

            return uid != null ? BuildResponse(await bookBLL.CreateBook(book, Convert.ToInt32(uid))) : NoContent();
        }

        [Route("{id}")]
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateBook(ReqBook book, int id)
        {
            int? uid = RecoverUidSession();

            return uid != null ? BuildResponse(await bookBLL.UpdateBook(book, id, Convert.ToInt32(uid))) : NoContent();
        }

        [Route("byupdatedat/{UpdatedAt}")]
        [HttpGet]
        [Authorize]
        public IActionResult GetBookByUpdatedAt(DateTime updatedAt)
        {
            int? uid = RecoverUidSession();

            return uid != null ? BuildResponse(bookBLL.GetByUpdatedAt(updatedAt, Convert.ToInt32(uid))) : NoContent();
        }
    }
}

