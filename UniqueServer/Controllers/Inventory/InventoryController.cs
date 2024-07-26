using BaseModels;
using InventoryBLL.Interfaces;
using InventoryModels.Req;
using InventoryModels.Res.Item;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace UniqueServer.Controllers.Inventory
{
    [Route("[Controller]")]
    [ApiController]
    [EnableRateLimiting("fixed")]
    [Authorize]
    public class InventoryController(ISubCategoryBLL subCategoryBLL, ICategoryBLL categoryBLL,
        IItemSituationBLL itemSituationBLL, IAcquisitionTypeBLL acquisitionTypeBLL, IItemBLL itemBLL,
        IHostEnvironment hostingEnvironment) : BaseController
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
        public IActionResult CreateCategory(ReqCategory reqCategory) => BuildResponse(categoryBLL.Create(reqCategory, Uid));

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

        [Route("item/{id:int:min(1)}")]
        [HttpPut]
        public IActionResult UpdateItem(ReqItem reqItem, int id) => BuildResponse(itemBLL.UpdateItem(reqItem, Uid, id));

        [Route("item/{id:int:min(1)}")]
        [HttpGet]
        public IActionResult GetItemById(int id) => BuildResponse(itemBLL.GetById(Uid, id));

        [Route("item")]
        [HttpGet]
        public IActionResult GetItems(int page) => BuildResponse(itemBLL.GetAsync(Uid, page).Result);

        [Route("item/totals")]
        [HttpGet]
        public IActionResult GetItems() => BuildResponse(itemBLL.GetTotalItemsPagesAsync(Uid).Result);

        [Route("item/{id}")]
        [HttpDelete]
        public IActionResult DeleteItem(int id) => BuildResponse(itemBLL.DeleteItem(Uid, id, ReturnPath()));

        [Route("item/{id}/image")]
        [HttpPut]
        public IActionResult UploadItemImages(int id, IFormFile? file1, IFormFile? file2)
        {
            BaseResponse bLLResponse = itemBLL.GetById(Uid, id);

            ResItem? resItem = (bLLResponse.Content as ResItem);

            if (bLLResponse == null)
                return BadRequest();

            if (bLLResponse.Content != null)
            {
                string? fileName1;

                if (file1 != null)
                {
                    if (!ValidateFileExtension(file1)) return BadRequest("Image in invalid format");

                    if (resItem?.Image1 != null) fileName1 = resItem.Image1;
                    else fileName1 = Guid.NewGuid() + Path.GetExtension(file1.FileName).ToLower();

                    _ = SaveInLocalFolder(file1, fileName1);
                }
                else return BadRequest("Image 1 in invalid format");

                string? fileName2;

                if (file2 != null)
                {
                    if (!ValidateFileExtension(file2)) return BadRequest("Image in invalid format");

                    if (resItem?.Image2 != null) fileName2 = resItem.Image2;
                    else fileName2 = Guid.NewGuid() + Path.GetExtension(file2.FileName).ToLower();

                    _ = SaveInLocalFolder(file2, fileName2);
                }
                else
                    fileName2 = resItem?.Image2;

                return BuildResponse(itemBLL.UpdateItemFileNames(Uid, id, fileName1, fileName2));
            }
            else return BuildResponse(bLLResponse);
        }

        [Route("item/{id}/image/{imageName}")]
        [HttpGet]
        public async Task<IActionResult> GetItemImagesByIndex(int id, string imageName)
        {
            if (!await itemBLL.CheckItemImageNameAsync(Uid, id, imageName)) return BadRequest("Invalid Index");

            string path = ReturnPath();
            string fullPath = Path.Combine(path, imageName);

            if (!System.IO.File.Exists(fullPath)) return BadRequest("This file don't exist");

            MemoryStream memory = new();
            using (FileStream stream = new(fullPath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            string ext = Path.GetExtension(fullPath).ToLowerInvariant();
            return File(memory, GetMimeTypes()[ext], Path.GetFileName(fullPath));
        }

        [Route("item/{id}/image/{imageName}")]
        [HttpDelete]
        public IActionResult DeleteItemImageByImageName(int id, string imageName) => BuildResponse(itemBLL.DeleteItemImage(Uid, id, imageName, ReturnPath()));

        private string ReturnPath()
        {
            string path = Path.Combine(hostingEnvironment.ContentRootPath, "StaticFiles", "ItemsImages");

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return path;
        }

        private async Task<bool> SaveInLocalFolder(IFormFile file, string fileName)
        {
            string path = ReturnPath();

            using FileStream fileStream = new(Path.Combine(path, fileName), FileMode.Create, FileAccess.ReadWrite);
            await file.CopyToAsync(fileStream);
            return true;
        }

        private static Dictionary<string, string> GetMimeTypes()
            => new()
            {
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".gif", "image/gif"}
            };

        private bool ValidateFileExtension(IFormFile file)
        {
            string[] validContentTypes = ["image/jpg", "image/jpeg", "image/pjpeg", "image/png"];
            string[] validExtensions = [".jpg", ".png", ".jpeg", ".webp"];

            //if (!validContentTypes.Contains(file.ContentType)) return false;

            if (!validExtensions.Contains(Path.GetExtension(file.FileName).ToLower())) return false;

            return true;
        }

        #endregion
    }
}
