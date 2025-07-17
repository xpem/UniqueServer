using InventoryModels.DTOs;
using InventoryRepos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryRepos
{
    public class SubCategoryRepo(InventoryDbContext dbContext) : ISubCategoryRepo
    {
        public async Task<SubCategory?> GetById(int uid, int id) =>
            await dbContext.SubCategory.Where(x => (x.UserId == uid || x.UserId == null && x.SystemDefault) && x.Id == id && !x.Inactive).FirstOrDefaultAsync();

        //public async Task<SubCategory?> GetByIdWithCategory(int uid, int id) =>
        //    await dbContext.SubCategory.Include(x => x.Category).Where(x => (x.UserId == uid || x.UserId == null && x.SystemDefault) && x.Id == id).FirstOrDefaultAsync();

        public async Task<List<SubCategory>> GetByAfterUpdatedAtAsync(int uid, DateTime updatedAt, int page, int pageSize)
            => await dbContext.SubCategory.Where(x => (x.UserId == uid || x.UserId == null && x.SystemDefault) && x.UpdatedAt > updatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        public List<SubCategory>? GetByCategoryId(int uid, int categoryId) => [.. dbContext.SubCategory.Where(x => (x.UserId == uid || x.UserId == null && x.SystemDefault) && x.CategoryId == categoryId && !x.Inactive)];

        public SubCategory? GetByCategoryIdAndName(int uid, int categoryId, string name) => dbContext.SubCategory.Where(x => (x.UserId == uid || x.UserId == null && x.SystemDefault) && x.CategoryId == categoryId && x.Name == name && !x.Inactive).FirstOrDefault();

        public async Task<int> CreateAsync(SubCategory subCategory)
        {
            await dbContext.SubCategory.AddAsync(subCategory);

            return await dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(SubCategory subCategory)
        {

            subCategory.UpdatedAt = DateTime.Now;

            dbContext.ChangeTracker?.Clear();

            dbContext.SubCategory.Update(subCategory);

            return await dbContext.SaveChangesAsync();
        }

        //public int Delete(SubCategory subCategory)
        //{
        //    dbContext.ChangeTracker?.Clear();

        //    dbContext.SubCategory.Remove(subCategory);

        //    return dbContext.SaveChanges();
        //}
    }
}
