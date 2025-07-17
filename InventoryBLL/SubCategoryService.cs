using BaseModels;
using InventoryBLL.Interfaces;
using InventoryModels.DTOs;
using InventoryModels.Req;
using InventoryModels.Res;
using InventoryRepos.Interfaces;

namespace InventoryBLL
{
    public class SubCategoryService(ISubCategoryRepo subCategoryRepo) : ISubCategoryService
    {
        readonly int pageSize = 50;

        public async Task<BaseResponse> CreateSubCategoryAsync(ReqSubCategory reqSubCategory, int uid)
        {
            try
            {
                string? validateError = reqSubCategory.Validate();
                if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(null, validateError);

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
                    SystemDefault = false
                };

                string? existingItemMsg = ValidateExistingSubCategory(subCategory);

                if (existingItemMsg != null)
                {
                    return new BaseResponse(null, existingItemMsg);
                }

                int respExec = await subCategoryRepo.CreateAsync(subCategory);

                if (respExec == 1)
                {
                    ResSubCategory resSubCategory = new() { Id = subCategory.Id, Name = subCategory.Name, IconName = subCategory.IconName, CategoryId = subCategory.CategoryId, SystemDefault = subCategory.SystemDefault };
                    return new BaseResponse(resSubCategory);
                }
                else
                    return new BaseResponse(null, "Não foi possivel adicionar.");
            }
            catch { throw; }
        }

        public async Task<BaseResponse> InactiveSubCategoryAsync(int uid, int id)
        {
            try
            {
                SubCategory? subCategory = await subCategoryRepo.GetById(uid, id);

                if (subCategory == null)
                    return new BaseResponse(null, "Invalid id");

                if (subCategory.SystemDefault)
                    return new BaseResponse(null, "It's not possible delete a system default Sub Category");

                subCategory.Inactive = true;

                int respExec = await subCategoryRepo.UpdateAsync(subCategory);

                if (respExec == 1)
                    return new BaseResponse(null);
                else
                    return new BaseResponse(null, "Não foi possivel deletar.");
            }
            catch { throw; }
        }

        public async Task<BaseResponse> UpdateSubCategoryAsync(ReqSubCategory reqSubCategory, int uid, int id)
        {
            try
            {
                string? validateError = reqSubCategory.Validate();
                if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(null, validateError);

                SubCategory? oldSubCategory = await subCategoryRepo.GetById(uid, id);

                if (oldSubCategory == null)
                    return new BaseResponse(null, "Invalid id");

                if (oldSubCategory.SystemDefault)
                    return new BaseResponse(null, "It's not possible edit a system default Sub Category");

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
                    SystemDefault = oldSubCategory.SystemDefault
                };

                string? existingItemMsg = null;

                if (oldSubCategory.Name != subCategory.Name)
                    existingItemMsg = ValidateExistingSubCategory(subCategory, id);

                if (existingItemMsg != null)
                {
                    return new BaseResponse(null, existingItemMsg);
                }

                int respExec = await subCategoryRepo.UpdateAsync(subCategory);

                if (respExec == 1)
                {
                    ResSubCategory resSubCategory = new() { Id = subCategory.Id, Name = subCategory.Name, IconName = subCategory.IconName, CategoryId = subCategory.CategoryId, SystemDefault = subCategory.SystemDefault };
                    return new BaseResponse(resSubCategory);
                }
                else
                    return new BaseResponse(null, "Não foi possivel atualizar.");
            }
            catch { throw; }
        }

        public BaseResponse GetByCategoryId(int uid, int categoryId)
        {
            List<SubCategory>? subCategories = subCategoryRepo.GetByCategoryId(uid, categoryId);
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

            return new BaseResponse(resSubCategories);
        }

        public async Task<BaseResponse> GetById(int uid, int id)
        {
            SubCategory? subCategory = await subCategoryRepo.GetById(uid, id);
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

            return new BaseResponse(resSubCategory);
        }

        //public async Task<BaseResponse> GetByIdWithCategory(int uid, int id)
        //{
        //    SubCategory? subCategory = await subCategoryRepo.GetById(uid, id);
        //    ResSubCategoryWithCategory? resSubCategory = null;

        //    if (subCategory is not null)
        //        resSubCategory = new()
        //        {
        //            Id = subCategory.Id,
        //            Name = subCategory.Name,
        //            //CategoryId = subCategory.CategoryId,
        //            Category = new ResCategory
        //            {
        //                Id = subCategory.Category.Id,
        //                Name = subCategory.Category.Name,
        //                Color = subCategory.Category.Color,
        //                SystemDefault = subCategory.Category.SystemDefault
        //            },
        //            IconName = subCategory.IconName,
        //            SystemDefault = subCategory.SystemDefault,
        //        };

        //    return new BaseResponse(resSubCategory);
        //}

        protected string? ValidateExistingSubCategory(SubCategory subCategory, int? id = null)
        {
            SubCategory? respSubCategory = subCategoryRepo.GetByCategoryIdAndName(subCategory.UserId.Value, subCategory.CategoryId, subCategory.Name);

            if ((respSubCategory is not null) && ((id is not null && respSubCategory.Id != id) || (id is null)))
                return "A Sub Category with this Name has already been added to this Category";

            return null;
        }

        public async Task<BaseResponse> GetByAfterUpdatedAtAsync(int uid, int page, DateTime updatedAt)
        {
            if (page <= 0)
                return new BaseResponse(null, "Invalid page");

            List<SubCategory> subCategories = await subCategoryRepo.GetByAfterUpdatedAtAsync(uid, updatedAt, page, pageSize);

            List<ResSubCategory> resSubCategories = [];

            if (subCategories != null)
                foreach (SubCategory subCategory in subCategories)
                    resSubCategories.Add(
                        new()
                        {
                            Id = subCategory.Id,
                            Name = subCategory.Name,
                            CategoryId = subCategory.CategoryId,
                            IconName = subCategory.IconName,
                            UpdatedAt = subCategory.UpdatedAt,
                            SystemDefault = subCategory.SystemDefault
                        });

            return new BaseResponse(resSubCategories);
        }
    }
}
