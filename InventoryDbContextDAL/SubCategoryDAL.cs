using InventoryDAL.Interfaces;
using InventoryDbContextDAL;
using InventoryModels;

namespace InventoryDAL
{
    public class SubCategoryDAL(InventoryDbContext dbContext) : ISubCategoryDAL
    {
        public SubCategory? GetById(int uid, int id) => dbContext.SubCategory.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Id == id).FirstOrDefault();

        public List<SubCategory>? GetByCategoryId(int uid, int categoryId) => dbContext.SubCategory.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.CategoryId == categoryId).ToList();

        public SubCategory? GetByCategoryIdAndName(int uid, int categoryId, string name) => dbContext.SubCategory.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.CategoryId == categoryId && x.Name == name).FirstOrDefault();

        public int Create(SubCategory subCategory)
        {
            dbContext.SubCategory.AddAsync(subCategory);

            return dbContext.SaveChanges();
        }

        public int Update(SubCategory subCategory)
        {
            dbContext.ChangeTracker?.Clear();

            dbContext.SubCategory.Update(subCategory);

            return dbContext.SaveChanges();
        }

        public int Delete(SubCategory subCategory)
        {
            dbContext.ChangeTracker?.Clear();

            dbContext.SubCategory.Remove(subCategory);

            return dbContext.SaveChanges();
        }
    }
}
