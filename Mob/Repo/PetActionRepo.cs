using Microsoft.EntityFrameworkCore;
using MobModels;

namespace MobRepo
{
    public class PetActionRepo(IDbContextFactory<MobDbCtx> dbCtx) : IPetActionRepo
    {
        public async Task<int> CreateAsync(PetAction action)
        {
            await using var dbContext = await dbCtx.CreateDbContextAsync();
            dbContext.ChangeTracker?.Clear();
            await dbContext.PetAction.AddAsync(action);
            return await dbContext.SaveChangesAsync();
        }

        public async Task<List<PetAction>> GetByPetIdAsync(int petId, int limit = 50)
        {
            await using var dbContext = await dbCtx.CreateDbContextAsync();
            return await dbContext.PetAction
                .Where(x => x.PetId == petId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> CountByPetIdAsync(int petId)
        {
            await using var dbContext = await dbCtx.CreateDbContextAsync();
            return await dbContext.PetAction
                .Where(x => x.PetId == petId)
                .CountAsync();
        }

        public async Task<List<PetAction>> GetByPetIdAndTypeAsync(int petId, PetActionType actionType, int limit = 20)
        {
            await using var dbContext = await dbCtx.CreateDbContextAsync();
            return await dbContext.PetAction
                .Where(x => x.PetId == petId && x.ActionType == actionType)
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }
    }
}
