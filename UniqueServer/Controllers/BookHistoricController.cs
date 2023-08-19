using BookshelfBLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniqueServer.Controllers
{
    [Route("[Controller]")]
    [ApiController]
    public class BookHistoricController : BaseController
    {
        private readonly IBookHistoricBLL bookHistoricBLL;

        public BookHistoricController(IBookHistoricBLL bookHistoricBLL)
        {
            this.bookHistoricBLL = bookHistoricBLL;
        }

        [Route("bycreatedat/{createdAt}")]
        [HttpGet]
        [Authorize]
        public IActionResult GetBookHistoricByUpdatedAt(string createdAt)
        {
            //format 2023-06-10T21:53:28.331Z

            DateTime dtCreatedAt = DateTime.Parse(createdAt);

            int? uid = RecoverUidSession();

            return uid != null ? BuildResponse(bookHistoricBLL.GetByCreatedAt(dtCreatedAt, Convert.ToInt32(uid))) : NoContent();
        }
    }
}
