using InventoryModels.DTOs;
using InventoryRepos;
using InventoryRepos.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace InventoryRepo
{
    public class CategoryRepo(IDbContextFactory<InventoryDbCtx> dbCtx) : ICategoryRepo
    {
        public async Task<int> CreateAsync(Category category)
        {
            using var context = dbCtx.CreateDbContext();
            await context.Category.AddAsync(category);
            return await context.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Category category)
        {
            using var context = dbCtx.CreateDbContext();
            context.ChangeTracker?.Clear();
            context.Category.Remove(category);
            return await context.SaveChangesAsync();
        }

        public async Task<List<Category>?> GetAsync(int uid)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.Category
                .Where(x => x.UserId == uid || (x.UserId == null && x.SystemDefault))
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int uid, int id)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.Category
                .Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Category?> GetByNameAsync(int uid, string name)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.Category
                .Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Name == name)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Category>?> GetByIdWithSubCategoriesAsync(int uid, int? id = null)
        {
            using var context = dbCtx.CreateDbContext();
            if (id is null)
                return await context.Category
                    .Where(x => x.UserId == uid || (x.UserId == null && x.SystemDefault))
                    .Include(x => x.SubCategories!.Where(sc => !sc.Inactive && (sc.UserId == uid || (sc.UserId == null && sc.SystemDefault))))
                    .OrderBy(x => x.Id)
                    .ToListAsync();
            else
                return await context.Category
                    .Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Id == id)
                    .Include(x => x.SubCategories!.Where(sc => !sc.Inactive && (sc.UserId == uid || (sc.UserId == null && sc.SystemDefault))))
                    .OrderBy(x => x.Id)
                    .ToListAsync();
        }

        public async Task<int> UpdateAsync(Category category)
        {
            using var context = dbCtx.CreateDbContext();
            context.ChangeTracker?.Clear();
            context.Category.Update(category);
            return await context.SaveChangesAsync();
        }
    }
}
