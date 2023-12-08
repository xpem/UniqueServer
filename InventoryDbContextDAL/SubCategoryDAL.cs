using InventoryDbContextDAL;
using InventoryModels;
using Microsoft.EntityFrameworkCore;

namespace InventoryDAL
{
    public class SubCategoryDAL(InventoryDbContext dbContext) : ISubCategoryDAL
    {
        public SubCategory? GetById(int uid, int id) => dbContext.SubCategory.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Id == id).FirstOrDefault();

        public List<SubCategory>? GetByCategoryId(int uid, int categoryId) => dbContext.SubCategory.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.CategoryId == categoryId).ToList();

        public SubCategory? GetByCategoryIdAndName(int uid, int categoryId, string name) => dbContext.SubCategory.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.CategoryId == categoryId && x.Name == name).FirstOrDefault();

        public async Task<int> CreateSubCategoryAsync(SubCategory subCategory)
        {
            await dbContext.SubCategory.AddAsync(subCategory);

            return await dbContext.SaveChangesAsync();
        }

        public int UpdateSubCategory(SubCategory subCategory)
        {
            dbContext.ChangeTracker?.Clear();

            dbContext.SubCategory.Update(subCategory);

            return dbContext.SaveChanges();
        }
    }
}
