using InventoryBLL.Interfaces;
using InventoryModels.Req;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniqueServer.Controllers.Inventory
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class InventoryController(ISubCategoryBLL subCategoryBLL, ICategoryBLL categoryBLL,IItemSituationBLL itemSituationBLL,IAcquisitionTypeBLL acquisitionTypeBLL) : BaseController
    {

        //[Route("")]
        //[HttpGet]
        ////[Authorize]
        //public async Task<IActionResult> CreateCategory()
        //{
        //    ReqCategory reqCategory = new() { Name = string.Empty, Color = string.Empty };

        //    await categoryBLL.CreateCategory(reqCategory);
        //    //int? uid = RecoverUidSession();

        //    //return uid != null ? BuildResponse(await bookBLL.CreateBook(book, Convert.ToInt32(uid))) : NoContent();

        //    return NoContent();
        //}

        [Route("subcategory")]
        [HttpPost]
        public IActionResult CreateSubCategory(ReqSubCategory reqSubCategory) => BuildResponse(subCategoryBLL.CreateSubCategory(reqSubCategory, Uid));

        [Route("subcategory/{id}")]
        [HttpPut]
        public IActionResult UpdateSubCategory(ReqSubCategory reqSubCategory, int id) => BuildResponse(subCategoryBLL.UpdateSubCategory(reqSubCategory, Uid, id));

        [Route("subcategory/{id}")]
        [HttpDelete]
        public IActionResult DeleteSubCategory(int id) => BuildResponse(subCategoryBLL.DeleteSubCategory(Uid, id));

        [Route("subcategory/{id}")]
        [HttpGet]
        public IActionResult GetSubCategoryById(int id) => BuildResponse(subCategoryBLL.GetById(Uid, id));

        [Route("subcategory/category/{categoryId}")]
        [HttpGet]
        public IActionResult GetSubCategoriesByCategoryId(int categoryId) => BuildResponse(subCategoryBLL.GetByCategoryId(Uid, categoryId));

        [Route("category")]
        [HttpGet]
        public IActionResult GetCategories() => BuildResponse(categoryBLL.Get(Uid));

        [Route("category/subcategory")]
        [HttpGet]
        public IActionResult GetCategoryById() => BuildResponse(categoryBLL.GetWithSubCategories(Uid));

        [Route("acquisitiontype")]
        [HttpGet]
        public IActionResult GetAcquisitionTypes() => BuildResponse(acquisitionTypeBLL.Get(Uid));

        [Route("itemsituation")]
        [HttpGet]
        public IActionResult GetItemSituations() => BuildResponse(itemSituationBLL.Get(Uid));

    }
}
