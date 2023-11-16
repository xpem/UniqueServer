using BookshelfBLL;
using BookshelfModels.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
        public IActionResult GetBookByUpdatedAt(string updatedAt)
        {
            //format 2023-06-10T21:53:28.331Z

            DateTime dtUpdatedAt = DateTime.Parse(updatedAt);

            int? uid = RecoverUidSession();

            return uid != null ? BuildResponse(bookBLL.GetByUpdatedAt(dtUpdatedAt, Convert.ToInt32(uid))) : NoContent();
        }

        [Route("bookstatus/{id}")]
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateBookStatus(ReqBookStatus reqBookStatus, int id)
        {
            int? uid = RecoverUidSession();

            return uid != null ? BuildResponse(await bookBLL.UpdateBookStatusAsync(reqBookStatus, id, Convert.ToInt32(uid))) : NoContent();
        }


        [Route("inactivate/{id}")]
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> InactivateBook(int id)
        {
            int? uid = RecoverUidSession();

            return uid != null ? BuildResponse(await bookBLL.InactivateBook(id, Convert.ToInt32(uid))) : NoContent();
        }
    }
}

