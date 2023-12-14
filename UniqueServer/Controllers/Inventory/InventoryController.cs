using InventoryBLL;
using InventoryBLL.Interfaces;
using InventoryModels.Req;
using InventoryModels.Res;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace UniqueServer.Controllers.Inventory
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    public class InventoryController(ISubCategoryBLL subCategoryBLL, ICategoryBLL categoryBLL) : BaseController
    {
        #region subcategory

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

        #endregion

        #region category

        [Route("category")]
        [HttpGet]
        public IActionResult GetCategories() => BuildResponse(categoryBLL.Get(Uid));

        [Route("category/{id}")]
        [HttpGet]
        public IActionResult GetCategoryById(int id) => BuildResponse(categoryBLL.GetById(Uid, id));

        [Route("category")]
        [HttpPost]
        public IActionResult CreateCategory(ReqCategory reqCategory) => BuildResponse(categoryBLL.CreateCategory(reqCategory, Uid));

        [Route("category/{id}")]
        [HttpPut]
        public IActionResult UpdateCategory(ReqCategory reqCategory, int id) => BuildResponse(categoryBLL.UpdateCategory(reqCategory, Uid, id));

        [Route("category/{id}")]
        [HttpDelete]
        public IActionResult DeleteCategory(int id) => BuildResponse(categoryBLL.DeleteCategory(Uid, id));

        [Route("category/subcategory")]
        [HttpGet]
        public IActionResult GetCategoryById() => BuildResponse(categoryBLL.GetWithSubCategories(Uid));

    }
}
