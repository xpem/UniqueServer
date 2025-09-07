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
    public class InventoryController(ISubCategoryService subCategoryService, ICategoryService categoryService,
        IItemSituationService itemSituationService, IAcquisitionTypeService acquisitionTypeService, IItemService itemService,
        IHostEnvironment hostingEnvironment) : BaseController
    {
        #region subcategory

        [Route("subcategory")]
        [HttpPost]
        public async Task<IActionResult> CreateSubCategoryAsync(ReqSubCategory reqSubCategory) => BuildResponse(await subCategoryService.CreateSubCategoryAsync(reqSubCategory, Uid));

        [Route("subcategory/{id}")]
        [HttpPut]
        public async Task<IActionResult> UpdateSubCategoryAsync(ReqSubCategory reqSubCategory, int id) => BuildResponse(await subCategoryService.UpdateSubCategoryAsync(reqSubCategory, Uid, id));

        [Route("subcategory/{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteSubCategoryAsync(int id) => BuildResponse(await subCategoryService.InactiveSubCategoryAsync(Uid, id));

        [Route("subcategory/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetSubCategoryById(int id) => BuildResponse(await subCategoryService.GetById(Uid, id));

        [Route("subcategory/category/{categoryId}")]
        [HttpGet]
        public IActionResult GetSubCategoriesByCategoryId(int categoryId) => BuildResponse(subCategoryService.GetByCategoryId(Uid, categoryId));

        [Route("subCategory/byAfterUpdatedAt/{updatedAt}/{page}")]
        [HttpGet]
        public async Task<IActionResult> GetSubCategoriesByAfterUpdatedAt(int page, string updatedAt)
        {
            //format 2023-06-10T21:53:28.331Z
            return BuildResponse(await subCategoryService.GetByAfterUpdatedAtAsync(Uid, page, DateTime.Parse(updatedAt)));
        }
        #endregion

        #region category

        [Route("category")]
        [HttpGet]
        public IActionResult GetCategories() => BuildResponse(categoryService.Get(Uid));

        [Route("category/{id}")]
        [HttpGet]
        public IActionResult GetCategoryById(int id) => BuildResponse(categoryService.GetById(Uid, id));

        [Route("category")]
        [HttpPost]
        public IActionResult CreateCategory(ReqCategory reqCategory) => BuildResponse(categoryService.Create(reqCategory, Uid));

        [Route("category/{id}")]
        [HttpPut]
        public IActionResult UpdateCategory(ReqCategory reqCategory, int id) => BuildResponse(categoryService.UpdateCategory(reqCategory, Uid, id));

        [Route("category/{id}")]
        [HttpDelete]
        public IActionResult DeleteCategory(int id) => BuildResponse(categoryService.DeleteCategory(Uid, id));

        [Route("category/subcategory")]
        [HttpGet]
        public async Task<IActionResult> GetCategoriesWithSubCategories() => BuildResponse(await categoryService.GetByIdWithSubCategories(Uid));

        [Route("category/{id}/subcategory")]
        [HttpGet]
        public async Task<IActionResult> GetCategoriyWithSubCategories(int id) => BuildResponse(await categoryService.GetByIdWithSubCategories(Uid, id));

        #endregion

        [Route("acquisitiontype")]
        [HttpGet]
        public async Task<IActionResult> GetAcquisitionTypes() => BuildResponse(await acquisitionTypeService.Get(Uid));

        [Route("itemsituation")]
        [HttpGet]
        public async Task<IActionResult> GetItemSituations() => BuildResponse(await itemSituationService.Get(Uid));

        #region item

        [Route("item")]
        [HttpPost]
        public IActionResult CreateItem(ReqItem reqItem) => BuildResponse(itemService.CreateItem(reqItem, Uid));

        [Route("item/{id:int:min(1)}")]
        [HttpPut]
        public IActionResult UpdateItem(ReqItem reqItem, int id) => BuildResponse(itemService.UpdateItem(reqItem, Uid, id));

        [Route("item/configs")]
        [HttpGet]
        public async Task<IActionResult> GetItemConfigs() => BuildResponse(await itemService.GetConfigs(Uid));

        [Route("item/{id:int:min(1)}")]
        [HttpGet]
        public IActionResult GetItemById(int id) => BuildResponse(itemService.GetById(Uid, id));

        [Route("item/totals")]
        [HttpGet]
        public IActionResult GetTotalItems() => BuildResponse(itemService.GetTotalItemsPagesAsync(Uid).Result);

        [Route("item/totals/search")]
        [HttpGet]
        public IActionResult GetTotalItemsBySearch([FromBody] ReqSearchItem reqSearchItem) => BuildResponse(itemService.GetTotalItemsPagesBySearchAsync(Uid, reqSearchItem).Result);

        [Route("item")]
        [HttpGet]
        public IActionResult GetItems([FromQuery] int page)
        {
            return BuildResponse(itemService.GetAsync(Uid, page).Result);
        }

        [Route("item/search")]
        [HttpPost]
        public IActionResult GetItemsBySearch([FromQuery] int page, [FromBody] ReqSearchItem reqSearchItem)
        {
            return BuildResponse(itemService.GetBySearch(Uid, page, reqSearchItem).Result);
        }

        [Route("item/{id}")]
        [HttpDelete]
        public IActionResult DeleteItem(int id) => BuildResponse(itemService.DeleteItem(Uid, id, ReturnPath()));

        [Route("item/{id}/image")]
        [HttpPut]
        public IActionResult UploadItemImages(int id, IFormFile? file1, IFormFile? file2)
        {
            BaseResponse bLLResponse = itemService.GetById(Uid, id);

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

                return BuildResponse(itemService.UpdateItemFileNames(Uid, id, fileName1, fileName2));
            }
            else return BuildResponse(bLLResponse);
        }

        [Route("item/{id}/image/{imageName}")]
        [HttpGet]
        public async Task<IActionResult> GetItemImagesByIndex(int id, string imageName)
        {
            if (!await itemService.CheckItemImageNameAsync(Uid, id, imageName)) return BadRequest("Invalid Index");

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
        public IActionResult DeleteItemImageByImageName(int id, string imageName) => BuildResponse(itemService.DeleteItemImage(Uid, id, imageName, ReturnPath()));

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

        private static bool ValidateFileExtension(IFormFile file)
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
