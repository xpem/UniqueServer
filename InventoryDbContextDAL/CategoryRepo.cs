using InventoryModels.DTOs;
using InventoryRepos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryRepos
{
    public class CategoryRepo(InventoryDbContext dbContext) : ICategoryRepo
    {
        public async Task<int> CreateAsync(Category category)
        {
            await dbContext.Category.AddAsync(category);
            return await dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Category category)
        {
            dbContext.ChangeTracker?.Clear();
            dbContext.Category.Remove(category);
            return await dbContext.SaveChangesAsync();
        }

        public async Task<List<Category>?> GetAsync(int uid)
        {
            return await dbContext.Category
                .Where(x => x.UserId == uid || (x.UserId == null && x.SystemDefault))
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int uid, int id)
        {
            return await dbContext.Category
                .Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Category?> GetByNameAsync(int uid, string name)
        {
            return await dbContext.Category
                .Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Name == name)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Category>?> GetByIdWithSubCategoriesAsync(int uid, int? id = null)
        {
            if (id is null)
                return await dbContext.Category
                    .Where(x => x.UserId == uid || (x.UserId == null && x.SystemDefault))
                    .Include(x => x.SubCategories!.Where(sc => !sc.Inactive && (sc.UserId == uid || (sc.UserId == null && sc.SystemDefault))))
                    .OrderBy(x => x.Id)
                    .ToListAsync();
            else
                return await dbContext.Category
                    .Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Id == id)
                    .Include(x => x.SubCategories!.Where(sc => !sc.Inactive && (sc.UserId == uid || (sc.UserId == null && sc.SystemDefault))))
                    .OrderBy(x => x.Id)
                    .ToListAsync();
        }

        public async Task<int> UpdateAsync(Category category)
        {
            dbContext.ChangeTracker?.Clear();
            dbContext.Category.Update(category);
            return await dbContext.SaveChangesAsync();
        }
    }
}
