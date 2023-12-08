using BaseModels;
using InventoryBLL.Interfaces;
using InventoryDAL;
using InventoryModels;
using InventoryModels.Req;
using InventoryModels.Res;

namespace InventoryBLL
{
    public class CategoryBLL(ICategoryDAL categoryDAL) : ICategoryBLL
    {
        public BLLResponse CreateCategory(ReqCategory reqCategory, int uid)
        {
            throw new NotImplementedException();
        }

        public BLLResponse DeleteCategory(int uid, int id)
        {
            throw new NotImplementedException();
        }

        public BLLResponse Get(int uid)
        {
            List<Category>? categories = categoryDAL.Get(uid);
            List<ResCategory> resCategories = [];

            if (categories != null && categories.Count > 0)
                foreach (var category in categories)
                    resCategories.Add(
                        new()
                        {
                            Id = category.Id,
                            Name = category.Name,
                            Color = category.Color,
                            SystemDefault = category.SystemDefault
                        });

            return new BLLResponse() { Content = resCategories };
        }

        public BLLResponse GetById(int uid, int id)
        {
            throw new NotImplementedException();
        }

        public BLLResponse GetWithSubCategories()
        {
            throw new NotImplementedException();
        }

        public BLLResponse UpdateCategory(ReqCategory reqCategory, int uid, int id)
        {
            throw new NotImplementedException();
        }
    }
}