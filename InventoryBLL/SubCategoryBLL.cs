using BaseModels;
using InventoryBLL.Interfaces;
using InventoryDAL;
using InventoryModels;
using InventoryModels.Req;
using InventoryModels.Res;

namespace InventoryBLL
{
    public class SubCategoryBLL(ISubCategoryDAL subCategoryDAL) : ISubCategoryBLL
    {
        public async Task<BLLResponse> CreateSubCategory(ReqSubCategory reqSubCategory, int uid)
        {
            try
            {
                string? validateError = reqSubCategory.Validate();
                if (!string.IsNullOrEmpty(validateError)) return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = validateError } };

                SubCategory subCategory = new()
                {
                    Name = reqSubCategory.Name,
                    IconName = reqSubCategory.IconName,
                    UserId = uid,
                    CategoryId = reqSubCategory.CategoryId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    SystemDefault = false,
                };

                string? existingItemMsg = ValidateExistingSubCategory(subCategory);

                if (existingItemMsg != null)
                {
                    return new BLLResponse()
                    {
                        Content = null,
                        Error = new ErrorMessage() { Error = existingItemMsg }
                    };
                }

                var respExec = await subCategoryDAL.ExecuteCreateSubCategoryAsync(subCategory);

                if (respExec == 1)
                {
                    ResSubCategory resSubCategory = new() { Id = subCategory.Id, Name = subCategory.Name, IconName = subCategory.IconName, CategoryId = subCategory.CategoryId, SystemDefault = subCategory.SystemDefault };
                    return new BLLResponse { Content = resSubCategory, Error = null };
                }
                else
                    return new BLLResponse { Content = null, Error = new ErrorMessage() { Error = "Não foi possivel adicionar." } };
            }
            catch { throw; }
        }

        public BLLResponse DeleteSubCategory(int uid, int subCategoryId)
        {
            throw new NotImplementedException();
        }

        public BLLResponse UpdateSubCategory(ReqSubCategory reqSubCategory, int uid, int id)
        {
            throw new NotImplementedException();
        }

        public BLLResponse GetByCategoryId(int uid, int categoryId)
        {
            List<SubCategory>? subCategories = subCategoryDAL.GetByCategoryId(uid, categoryId);
            List<ResSubCategory>? resSubCategories = [];

            if (subCategories != null)
                foreach (SubCategory subCategory in subCategories)
                    resSubCategories.Add(
                        new()
                        {
                            Id = subCategory.Id,
                            Name = subCategory.Name,
                            CategoryId = subCategory.CategoryId,
                            IconName = subCategory.IconName,
                            SystemDefault = subCategory.SystemDefault,
                        });

            return new BLLResponse() { Content = resSubCategories };
        }

        public BLLResponse GetById(int uid, int id)
        {
            SubCategory? subCategory = subCategoryDAL.GetById(uid, id);
            ResSubCategory? resSubCategory = null;

            if (subCategory is not null)
                resSubCategory = new()
                {
                    Id = subCategory.Id,
                    Name = subCategory.Name,
                    CategoryId = subCategory.CategoryId,
                    IconName = subCategory.IconName,
                    SystemDefault = subCategory.SystemDefault,
                };

            return new BLLResponse() { Content = resSubCategory };
        }


        protected string? ValidateExistingSubCategory(SubCategory subCategory)
        {
            SubCategory? respSubCategory = subCategoryDAL.GetByCategoryIdAndName(subCategory.UserId.Value, subCategory.CategoryId, subCategory.Name);

            if (respSubCategory is not null)
                return "A Sub Category with this Name has already been added to this Category";

            return null;
        }

    }
}
