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

        public async Task<BaseResp> CreateSubCategoryAsync(ReqSubCategory reqSubCategory, int uid)
        {
            try
            {
                string? validateError = reqSubCategory.Validate();
                if (!string.IsNullOrEmpty(validateError)) return new BaseResp(ErrorCode.InvalidObject, validateError);

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

                string? existingItemMsg = await ValidateExistingSubCategory(subCategory);

                if (existingItemMsg != null)
                {
                    return new BaseResp(ErrorCode.ExistingObject, existingItemMsg);
                }

                int respExec = await subCategoryRepo.CreateAsync(subCategory);

                if (respExec == 1)
                {
                    ResSubCategory resSubCategory = new() { Id = subCategory.Id, Name = subCategory.Name, IconName = subCategory.IconName, CategoryId = subCategory.CategoryId, SystemDefault = subCategory.SystemDefault };
                    return new BaseResp(resSubCategory);
                }
                else
                    return new BaseResp(ErrorCode.ErrorCreatingObject, "Não foi possivel adicionar.");
            }
            catch { throw; }
        }

        public async Task<BaseResp> InactiveSubCategoryAsync(int uid, int id)
        {
            try
            {
                SubCategory? subCategory = await subCategoryRepo.GetById(uid, id);

                if (subCategory == null)
                    return new BaseResp(ErrorCode.InvalidId, "Invalid id");

                if (subCategory.SystemDefault)
                    return new BaseResp(ErrorCode.TryDeleteSystemDefaultObject, "It's not possible delete a system default Sub Category");

                subCategory.Inactive = true;

                int respExec = await subCategoryRepo.UpdateAsync(subCategory);

                if (respExec == 1)
                    return new BaseResp(null);
                else
                    return new BaseResp(ErrorCode.ErrorDeletingObject, "Não foi possivel deletar.");
            }
            catch { throw; }
        }

        public async Task<BaseResp> UpdateSubCategoryAsync(ReqSubCategory reqSubCategory, int uid, int id)
        {
            try
            {
                string? validateError = reqSubCategory.Validate();
                if (!string.IsNullOrEmpty(validateError)) return new BaseResp(ErrorCode.InvalidObject, validateError);

                SubCategory? oldSubCategory = await subCategoryRepo.GetById(uid, id);

                if (oldSubCategory == null)
                    return new BaseResp(ErrorCode.InvalidId, "Invalid id");

                if (oldSubCategory.SystemDefault)
                    return new BaseResp(ErrorCode.TryDeleteSystemDefaultObject, "It's not possible edit a system default Sub Category");

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
                    existingItemMsg = await ValidateExistingSubCategory(subCategory, id);

                if (existingItemMsg != null)
                {
                    return new BaseResp(ErrorCode.ExistingObject, existingItemMsg);
                }

                int respExec = await subCategoryRepo.UpdateAsync(subCategory);

                if (respExec == 1)
                {
                    ResSubCategory resSubCategory = new() { Id = subCategory.Id, Name = subCategory.Name, IconName = subCategory.IconName, CategoryId = subCategory.CategoryId, SystemDefault = subCategory.SystemDefault };
                    return new BaseResp(resSubCategory);
                }
                else
                    return new BaseResp(ErrorCode.ErrorUpdatingObject, "Não foi possivel atualizar.");
            }
            catch { throw; }
        }

        public async Task<BaseResp> GetByCategoryIdAsync(int uid, int categoryId)
        {
            List<SubCategory>? subCategories = await subCategoryRepo.GetByCategoryIdAsync(uid, categoryId);
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

            return new BaseResp(resSubCategories);
        }

        public async Task<BaseResp> GetByIdAsync(int uid, int id)
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

            return new BaseResp(resSubCategory);
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

        protected async Task<string?> ValidateExistingSubCategory(SubCategory subCategory, int? id = null)
        {
            SubCategory? respSubCategory = await subCategoryRepo.GetByCategoryIdAndNameAsync(subCategory.UserId.Value, subCategory.CategoryId, subCategory.Name);

            if ((respSubCategory is not null) && ((id is not null && respSubCategory.Id != id) || (id is null)))
                return "A Sub Category with this Name has already been added to this Category";

            return null;
        }

        public async Task<BaseResp> GetByAfterUpdatedAtAsync(int uid, int page, DateTime updatedAt)
        {
            if (page <= 0)
                return new BaseResp(ErrorCode.InvalidPage, "Invalid page");

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

            return new BaseResp(resSubCategories);
        }
    }
}
