﻿using BaseModels;
using InventoryBLL.Interfaces;
using InventoryDAL.Interfaces;
using InventoryModels;
using InventoryModels.Req;
using InventoryModels.Res;

namespace InventoryBLL
{
    public class CategoryBLL(ICategoryDAL categoryDAL, ISubCategoryDAL subCategoryDAL) : ICategoryBLL
    {
        public BLLResponse Create(ReqCategory reqCategory, int uid)
        {
            try
            {
                string? validateError = reqCategory.Validate();
                if (!string.IsNullOrEmpty(validateError)) return new BLLResponse(null, validateError);

                Category category = new()
                {
                    Name = reqCategory.Name,
                    Color = reqCategory.Color,
                    CreatedAt = DateTime.Now,
                    SystemDefault = false,
                    UserId = uid
                };

                string? existingItemMsg = ValidateExistingCategory(category);

                if (existingItemMsg != null)
                    return new BLLResponse(null, existingItemMsg);

                int respExec = categoryDAL.Create(category);

                if (respExec == 1)
                {
                    ResCategory resCategory = new()
                    {
                        Name = category.Name,
                        Color = category.Color,
                        SystemDefault = category.SystemDefault,
                        Id = category.Id
                    };
                    return new BLLResponse(resCategory);
                }
                else
                    return new BLLResponse(null, "Não foi possivel adicionar.");
            }
            catch { throw; }
        }

        public BLLResponse DeleteCategory(int uid, int id)
        {
            try
            {
                Category? category = categoryDAL.GetById(uid, id);

                if (category == null)
                    return new BLLResponse(null, "Invalid id");

                if (category.SystemDefault)
                    return new BLLResponse(null, "It's not possible delete a system default Sub Category");

                List<SubCategory>? subCategories = subCategoryDAL.GetByCategoryId(uid, category.Id);

                if (subCategories != null && subCategories.Count > 0)
                    return new BLLResponse(null, "It's not possible delete a Category with Sub Categories");

                int respExec = categoryDAL.Delete(category);

                if (respExec == 1)
                    return new BLLResponse(1);
                else
                    return new BLLResponse(null, "Não foi possivel atualizar.");
            }
            catch { throw; }
        }

        public BLLResponse Get(int uid)
        {
            // InventoryDbContextDAL.InventoryInitializeDB.CreateInitiaValues();

            List<Category>? categories = categoryDAL.Get(uid);
            List<ResCategory> resCategories = [];

            if (categories != null && categories.Count > 0)
                foreach (Category category in categories)
                    resCategories.Add(
                        new()
                        {
                            Id = category.Id,
                            Name = category.Name,
                            Color = category.Color,
                            SystemDefault = category.SystemDefault
                        });

            return new BLLResponse(resCategories);
        }

        public BLLResponse GetById(int uid, int id)
        {
            Category? category = categoryDAL.GetById(uid, id);
            ResCategory? resCategories = null;

            if (category is not null)
                resCategories = new()
                {
                    Id = category.Id,
                    Name = category.Name,
                    Color = category.Color,
                    SystemDefault = category.SystemDefault
                };

            return new BLLResponse(resCategories);
        }

        public BLLResponse GetWithSubCategories(int uid)
        {
            List<Category>? categoriesWithSubCategories = categoryDAL.GetWithSubCategories(uid);
            List<ResCategoryWithSubCategories> resCategoriesWithSubCategories = [];

            if (categoriesWithSubCategories != null && categoriesWithSubCategories.Count > 0)
                foreach (Category categoryWithSubCategories in categoriesWithSubCategories)
                {
                    List<ResSubCategory> resSubCategories = [];

                    if (categoryWithSubCategories.SubCategories is not null)
                        foreach (SubCategory subCategory in categoryWithSubCategories.SubCategories)
                            resSubCategories.Add(new ResSubCategory()
                            {
                                Id = subCategory.Id,
                                Name = subCategory.Name,
                                IconName = subCategory.IconName,
                                CategoryId = categoryWithSubCategories.Id,
                                SystemDefault = subCategory.SystemDefault,
                            });

                    resCategoriesWithSubCategories.Add(
                        new()
                        {
                            Id = categoryWithSubCategories.Id,
                            Name = categoryWithSubCategories.Name,
                            Color = categoryWithSubCategories.Color,
                            SystemDefault = categoryWithSubCategories.SystemDefault,
                            SubCategories = resSubCategories,
                        });
                }

            return new BLLResponse(resCategoriesWithSubCategories);
        }

        public BLLResponse UpdateCategory(ReqCategory reqCategory, int uid, int id)
        {
            try
            {
                string? validateError = reqCategory.Validate();
                if (!string.IsNullOrEmpty(validateError)) return new BLLResponse(null, validateError);

                Category? oldCategory = categoryDAL.GetById(uid, id);

                if (oldCategory == null)
                    return new BLLResponse(null, "Invalid id");

                if (oldCategory.SystemDefault)
                    return new BLLResponse(null, "It's not possible edit a system default Category");

                Category category = new()
                {
                    Name = reqCategory.Name,
                    Color = reqCategory.Color,
                    CreatedAt = oldCategory.CreatedAt,
                    UpdatedAt = DateTime.Now,
                    SystemDefault = oldCategory.SystemDefault,
                    UserId = oldCategory.UserId,
                    Id = oldCategory.Id,
                };

                string? existingItemMsg = ValidateExistingCategory(category, id);

                if (existingItemMsg != null)
                {
                    return new BLLResponse(null, existingItemMsg);
                }

                int respExec = categoryDAL.Update(category);

                if (respExec == 1)
                {
                    List<SubCategory>? subCategories = subCategoryDAL.GetByCategoryId(uid, category.Id);
                    List<ResSubCategory>? resSubCategories = [];

                    if (subCategories is not null)
                        foreach (SubCategory subCategory in subCategories)
                            resSubCategories.Add(new ResSubCategory()
                            {
                                Id = subCategory.Id,
                                Name = subCategory.Name,
                                IconName = subCategory.IconName,
                                CategoryId = category.Id,
                                SystemDefault = subCategory.SystemDefault,
                            });

                    ResCategoryWithSubCategories resCategoryWithSubCategories = new()
                    {
                        Id = category.Id,
                        SystemDefault = category.SystemDefault,
                        Name = category.Name,
                        Color = category.Name,
                        SubCategories = resSubCategories

                    };
                    return new BLLResponse(resCategoryWithSubCategories);
                }
                else
                    return new BLLResponse("Não foi possivel atualizar.");
            }
            catch { throw; }
        }

        protected string? ValidateExistingCategory(Category Category, int? id = null)
        {
            Category? respCategory = categoryDAL.GetByName(Category.UserId.Value, Category.Name);

            if ((respCategory is not null) && ((id is not null && respCategory.Id != id) || (id is null)))
                return "A Category with this Name has already been added.";

            return null;
        }
    }
}