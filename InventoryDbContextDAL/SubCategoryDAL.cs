using InventoryDbContextDAL;
using InventoryModels;
using Microsoft.EntityFrameworkCore;

namespace InventoryDAL
{
    public class SubCategoryDAL(InventoryDbContext inventoryDbContext) : ISubCategoryDAL
    {
        public SubCategory? GetById(int uid, int id) => inventoryDbContext.SubCategory.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Id == id).FirstOrDefault();

        public List<SubCategory>? GetByCategoryId(int uid, int categoryId) => inventoryDbContext.SubCategory.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.CategoryId == categoryId).ToList();

        public SubCategory? GetByCategoryIdAndName(int uid, int categoryId,string name) => inventoryDbContext.SubCategory.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.CategoryId == categoryId && x.Name == name).FirstOrDefault();

        public async Task<int> ExecuteCreateSubCategoryAsync(SubCategory subCategory)
        {
            await inventoryDbContext.SubCategory.AddAsync(subCategory);

            return await inventoryDbContext.SaveChangesAsync();
        }
    }
}
