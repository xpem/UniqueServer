using BaseModels;
using InventoryDbContextDAL;
using InventoryModels;

namespace InventoryDAL
{
    public class CategoryDAL(InventoryDbContext dbContext) : ICategoryDAL
    {
        public int Create(Category category)
        {
            throw new NotImplementedException();
        }

        public int Delete(int uid, int id)
        {
            throw new NotImplementedException();
        }

        public List<Category>? Get(int uid) => dbContext.Category.Where(x => x.UserId == uid || (x.UserId == null && x.SystemDefault)).ToList();

        public Category? GetById(int uid, int id) => dbContext.Category.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Id == id).FirstOrDefault();

        public Category? GetByName(int uid, string name) => dbContext.Category.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Name == name).FirstOrDefault();

        public List<Category>? GetWithSubCategories(int uid)
        {
            throw new NotImplementedException();
        }

        public int Update(Category category)
        {
            throw new NotImplementedException();
        }
    }
}
