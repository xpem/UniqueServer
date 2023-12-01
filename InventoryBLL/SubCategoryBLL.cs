using BaseModels;
using InventoryDAL;
using InventoryModels;
using InventoryModels.Req;
using InventoryModels.Res;

namespace InventoryBLL
{
    public class SubCategoryBLL(ISubCategoryDAL subCategoryDAL) : ISubCategoryBLL
    {
        public BLLResponse CreateSubCategory(ReqSubCategory reqSubCategory, int uid)
        {
            throw new NotImplementedException();
        }

        public BLLResponse DeleteSubCategory(int uid, int subCategoryId)
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

        public Task<BLLResponse> UpdateSubCategory(ReqSubCategory reqSubCategory, int uid, int id)
        {
            throw new NotImplementedException();
        }

        BLLResponse ISubCategoryBLL.UpdateSubCategory(ReqSubCategory reqSubCategory, int uid, int id)
        {
            throw new NotImplementedException();
        }
    }
}
