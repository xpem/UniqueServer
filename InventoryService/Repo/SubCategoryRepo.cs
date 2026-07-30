using InventoryModels.DTOs;
using InventoryRepos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryRepos
{
    public class SubCategoryRepo(IDbContextFactory<InventoryDbCtx> dbCtx) : ISubCategoryRepo
    {
        public async Task<SubCategory?> GetById(int uid, int id)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.SubCategory.Where(x => (x.UserId == uid || x.UserId == null && x.SystemDefault) && x.Id == id && !x.Inactive).FirstOrDefaultAsync();
        }

        //public async Task<SubCategory?> GetByIdWithCategory(int uid, int id) =>
        //    await dbContext.SubCategory.Include(x => x.Category).Where(x => (x.UserId == uid || x.UserId == null && x.SystemDefault) && x.Id == id).FirstOrDefaultAsync();

        public async Task<List<SubCategory>> GetByAfterUpdatedAtAsync(int uid, DateTime updatedAt, int page, int pageSize)
        {
            DateTime updatedAtUtc = DateTime.SpecifyKind(updatedAt, DateTimeKind.Utc);
            using var context = dbCtx.CreateDbContext();
            return await context.SubCategory.Where(x => (x.UserId == uid || x.UserId == null && x.SystemDefault) && x.UpdatedAt > updatedAtUtc).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<List<SubCategory>?> GetByCategoryIdAsync(int uid, int categoryId)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.SubCategory.Where(x => (x.UserId == uid || x.UserId == null && x.SystemDefault) && x.CategoryId == categoryId && !x.Inactive).ToListAsync();
        }

        public async Task<SubCategory?> GetByCategoryIdAndNameAsync(int uid, int categoryId, string name)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.SubCategory.Where(x => (x.UserId == uid || x.UserId == null && x.SystemDefault) && x.CategoryId == categoryId && x.Name == name && !x.Inactive).FirstOrDefaultAsync();
        }

        public async Task<int> CreateAsync(SubCategory subCategory)
        {
            using var context = dbCtx.CreateDbContext();
            await context.SubCategory.AddAsync(subCategory);

            return await context.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(SubCategory subCategory)
        {

            subCategory.UpdatedAt = DateTime.UtcNow;

            using var context = dbCtx.CreateDbContext();
            context.ChangeTracker?.Clear();

            context.SubCategory.Update(subCategory);

            return await context.SaveChangesAsync();
        }

        //public int Delete(SubCategory subCategory)
        //{
        //    dbContext.ChangeTracker?.Clear();

        //    dbContext.SubCategory.Remove(subCategory);

        //    return dbContext.SaveChanges();
        //}
    }
}
