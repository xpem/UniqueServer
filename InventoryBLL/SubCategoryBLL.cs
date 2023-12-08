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

                string iconName = "Tag";

                if (!string.IsNullOrEmpty(reqSubCategory.IconName))
                    iconName = reqSubCategory.IconName;

                SubCategory subCategory = new()
                {
                    Name = reqSubCategory.Name,
                    IconName = iconName,
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

                var respExec = await subCategoryDAL.CreateSubCategoryAsync(subCategory);

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
            try
            {
                string? validateError = reqSubCategory.Validate();
                if (!string.IsNullOrEmpty(validateError)) return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = validateError } };

                SubCategory? oldSubCategory = subCategoryDAL.GetById(uid, id);

                if (oldSubCategory == null)
                    return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = "Invalid id" } };

                if (oldSubCategory.SystemDefault)
                    return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = "It's not possible edit a system default Sub Category" } };

                string iconName = "Tag";

                if (!string.IsNullOrEmpty(reqSubCategory.IconName))
                    iconName = reqSubCategory.IconName;

                SubCategory subCategory = new()
                {
                    Id = oldSubCategory.Id,
                    Name = reqSubCategory.Name,
                    IconName = iconName,
                    UserId = oldSubCategory.UserId,
                    CategoryId = oldSubCategory.CategoryId,
                    CreatedAt = oldSubCategory.CreatedAt,
                    UpdatedAt = DateTime.Now,
                    SystemDefault = oldSubCategory.SystemDefault,
                };

                string? existingItemMsg = ValidateExistingSubCategory(subCategory, id);

                if (existingItemMsg != null)
                {
                    return new BLLResponse()
                    {
                        Content = null,
                        Error = new ErrorMessage() { Error = existingItemMsg }
                    };
                }

                var respExec = subCategoryDAL.UpdateSubCategory(subCategory);

                if (respExec == 1)
                {
                    ResSubCategory resSubCategory = new() { Id = subCategory.Id, Name = subCategory.Name, IconName = subCategory.IconName, CategoryId = subCategory.CategoryId, SystemDefault = subCategory.SystemDefault };
                    return new BLLResponse { Content = resSubCategory, Error = null };
                }
                else
                    return new BLLResponse { Content = null, Error = new ErrorMessage() { Error = "Não foi possivel atualizar." } };
            }
            catch { throw; }
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


        protected string? ValidateExistingSubCategory(SubCategory subCategory, int? id = null)
        {
            SubCategory? respSubCategory = subCategoryDAL.GetByCategoryIdAndName(subCategory.UserId.Value, subCategory.CategoryId, subCategory.Name);

            if ((respSubCategory is not null) && ((id is not null && respSubCategory.Id != id) || (id is null)))
                return "A Sub Category with this Name has already been added to this Category";

            return null;
        }

    }
}
