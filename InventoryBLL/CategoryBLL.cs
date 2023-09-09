using BaseModels;
using InventoryDbContextDAL;
using InventoryModels;
using InventoryModels.Req;

namespace InventoryBLL
{
    public class CategoryBLL : ICategoryBLL
    {
        private readonly InventoryDbContext inventoryDbContext;

        public CategoryBLL(InventoryDbContext inventoryDbContext)
        {
            this.inventoryDbContext = inventoryDbContext;
        }

        public Task<BLLResponse> CreateCategory(ReqCategory reqCategory)
        {
            throw new NotImplementedException();
        }

        public Task<BLLResponse> Get(int uid)
        {
            throw new NotImplementedException();
        }

        public Task<BLLResponse> GetById(int uid, int id)
        {
            throw new NotImplementedException();
        }

        public Task<BLLResponse> GetWithSubCategories()
        {
            throw new NotImplementedException();
        }
    }
}