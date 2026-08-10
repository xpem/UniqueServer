using Microsoft.EntityFrameworkCore;
using MobModels;

namespace MobRepo
{
    public class PetRepo(IDbContextFactory<MobDbCtx> dbCtx) : IPetRepo
    {
        public async Task<int> CreateAsync(Pet pet)
        {
            await using var dbContext = await dbCtx.CreateDbContextAsync();
            dbContext.ChangeTracker?.Clear();
            await dbContext.Pet.AddAsync(pet);
            return await dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(Pet pet)
        {
            await using var dbContext = await dbCtx.CreateDbContextAsync();
            dbContext.ChangeTracker?.Clear();
            pet.UpdatedAt = DateTime.UtcNow;
            dbContext.Pet.Update(pet);
            return await dbContext.SaveChangesAsync();
        }

        public async Task<Pet?> GetActiveByUserIdAsync(int userId)
        {
            await using var dbContext = await dbCtx.CreateDbContextAsync();
            return await dbContext.Pet
                .Where(x => x.UserId == userId && !x.IsDead)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<Pet?> GetByIdAsync(int id)
        {
            await using var dbContext = await dbCtx.CreateDbContextAsync();
            return await dbContext.Pet.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Pet>> GetAllByUserIdAsync(int userId)
        {
            await using var dbContext = await dbCtx.CreateDbContextAsync();
            return await dbContext.Pet
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var dbContext = await dbCtx.CreateDbContextAsync();
            dbContext.ChangeTracker?.Clear();
            await dbContext.Pet.Where(x => x.Id == id).ExecuteDeleteAsync();
        }
    }
}
