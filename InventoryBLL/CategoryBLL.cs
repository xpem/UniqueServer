using BaseModels;
using InventoryDbContextDAL;
using InventoryModels;
using InventoryModels.Req;

namespace InventoryBLL
{
    public class CategoryBLL(InventoryDbContext inventoryDbContext) : ICategoryBLL
    {
        public Task<BLLResponse> CreateCategory(ReqCategory reqCategory)
        {
            InventoryDbContextDAL.InventoryInitializeDB ini = new(inventoryDbContext);
            ini.CreateInitiaValues();

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