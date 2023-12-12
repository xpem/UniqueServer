using InventoryDAL.Interfaces;
using InventoryDbContextDAL;
using InventoryModels;
using Microsoft.EntityFrameworkCore;

namespace InventoryDAL
{
    public class CategoryDAL(InventoryDbContext dbContext) : ICategoryDAL
    {
        public int Create(Category category)
        {
            dbContext.Category.Add(category);
            return dbContext.SaveChanges();
        }

        public int Delete(Category category)
        {
            dbContext.ChangeTracker?.Clear();

            dbContext.Category.Remove(category);

            return dbContext.SaveChanges();
        }

        public List<Category>? Get(int uid) => dbContext.Category.Where(x => x.UserId == uid || (x.UserId == null && x.SystemDefault)).ToList();

        public Category? GetById(int uid, int id) => dbContext.Category.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Id == id).FirstOrDefault();

        public Category? GetByName(int uid, string name) => dbContext.Category.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Name == name).FirstOrDefault();

        public List<Category>? GetWithSubCategories(int uid)
            => dbContext.Category.Where(x => x.UserId == uid || (x.UserId == null && x.SystemDefault)).Include(x => x.SubCategories).OrderBy(x => x.Id).ToList();


        public int Update(Category category)
        {
            dbContext.ChangeTracker?.Clear();

            dbContext.Category.Update(category);

            return dbContext.SaveChanges();
        }
    }
}
