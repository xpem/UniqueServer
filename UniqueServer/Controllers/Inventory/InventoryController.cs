using InventoryBLL.Interfaces;
using InventoryModels.Req;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniqueServer.Controllers.Inventory
{
    [Route("[Controller]")]
    [ApiController]
    public class InventoryController(ICategoryBLL categoryBLL, ISubCategoryBLL subCategoryBLL) : BaseController
    {
        [Route("")]
        [HttpGet]
        //[Authorize]
        public async Task<IActionResult> CreateCategory()
        {
            ReqCategory reqCategory = new() { Name = string.Empty, Color = string.Empty };

            await categoryBLL.CreateCategory(reqCategory);
            //int? uid = RecoverUidSession();

            //return uid != null ? BuildResponse(await bookBLL.CreateBook(book, Convert.ToInt32(uid))) : NoContent();

            return NoContent();
        }

        [Route("subcategory/{id}")]
        [HttpGet]
        [Authorize]
        public IActionResult GetSubCategoryById(int id)
        {
            int? uid = RecoverUidSession();

            return uid != null ? BuildResponse(subCategoryBLL.GetById(Convert.ToInt32(uid), id)) : NoContent();
        }

        [Route("subcategory/category/{categoryId}")]
        [HttpGet]
        [Authorize]
        public IActionResult GetSubCategoriesByCategoryId(int categoryId)
        {
            int? uid = RecoverUidSession();

            return uid != null ? BuildResponse(subCategoryBLL.GetByCategoryId(Convert.ToInt32(categoryId), categoryId)) : NoContent();
        }
    }
}
