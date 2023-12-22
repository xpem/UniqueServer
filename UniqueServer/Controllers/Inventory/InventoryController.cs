using InventoryBLL.Interfaces;
using InventoryDAL;
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
    public class InventoryController(ISubCategoryBLL subCategoryBLL, ICategoryBLL categoryBLL, IItemSituationBLL itemSituationBLL, IAcquisitionTypeBLL acquisitionTypeBLL,IItemBLL itemBLL) : BaseController
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
        public IActionResult GetCategoriesWithSubCategories() => BuildResponse(categoryBLL.GetWithSubCategories(Uid));

        #endregion

        [Route("acquisitiontype")]
        [HttpGet]
        public IActionResult GetAcquisitionTypes() => BuildResponse(acquisitionTypeBLL.Get(Uid));

        [Route("itemsituation")]
        [HttpGet]
        public IActionResult GetItemSituations() => BuildResponse(itemSituationBLL.Get(Uid));

        #region item
        [Route("item")]
        [HttpPost]
        public IActionResult CreateItem(ReqItem reqItem) => BuildResponse(itemBLL.CreateItem(reqItem, Uid));

        [Route("item/{id}")]
        [HttpGet]
        public IActionResult GetItemById(int id) => BuildResponse(itemBLL.GetById(Uid, id));


        #endregion
        //https://stackoverflow.com/questions/32178012/want-to-save-a-image-to-a-folder-and-saving-the-url-in-database
        /*
          private readonly IHostEnvironment _hostingEnvironment;
    private readonly string _path = "";

    Constructor(IHostEnvironment hostingEnvironment){
        _hostingEnvironment = hostingEnvironment;
        _path = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", "Images");    
    }

    //your method that receives the image {
        string fileName = null;
        if (image != null) //type of image is IFormFile
        {
            Guid guid = Guid.NewGuid();
            fileName = $"{guid}.{image.FileName.Split('.').Last()}";
            await SaveInLocalFolder(image, fileName);
        }
    //}

    private async Task<bool> SaveInLocalFolder(IFormFile file, string fileName)
    {
        if (!Directory.Exists(_path))
        {
            Directory.CreateDirectory(_path);
        }
        using (var fileStream = new FileStream(Path.Combine(_path, fileName), FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
        return true;
    }
         */
    }
}
