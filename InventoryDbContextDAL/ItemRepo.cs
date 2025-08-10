using InventoryModels.DTOs;
using InventoryRepos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryRepos
{
    public class ItemRepo(InventoryDbContext dbContext) : IItemRepo
    {
        public Item? GetById(int uid, int id)
            => dbContext.Item.Where(x => x.Id == id && x.UserId == uid)
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.ItemSituation)
            .Include(x => x.AcquisitionType)
            .FirstOrDefault();

        public int Create(Item item)
        {
            dbContext.ChangeTracker?.Clear();
            dbContext.Item.Add(item);
            return dbContext.SaveChanges();
        }

        public int Update(Item item)
        {
            dbContext.ChangeTracker?.Clear();
            dbContext.Item.Update(item);
            return dbContext.SaveChanges();
        }

        public int UpdateFileNames(int uid, int id, string? fileName1, string? fileName2)
        {
            dbContext.ChangeTracker?.Clear();

            return dbContext.Item.Where(x => x.UserId == uid && x.Id == id).ExecuteUpdate(y => y
               .SetProperty(z => z.Image1, fileName1)
               .SetProperty(z => z.Image2, fileName2)
               .SetProperty(z => z.UpdatedAt, DateTime.Now)
               );
        }

        public Task<bool> CheckItemImageNameAsync(int uid, int id, string imageName) => dbContext.Item.AnyAsync(x => x.Id == id && x.UserId == uid && (x.Image1 == imageName || x.Image2 == imageName));

        public int Delete(Item item)
        {
            dbContext.ChangeTracker?.Clear();
            dbContext.Item.Remove(item);
            return dbContext.SaveChanges();
        }

        public async Task<int> GetTotalAsync(int uid, int[]? situationIds)
        {
            if (situationIds is not null && situationIds?.Length > 0)
            {
                return await dbContext.Item.CountAsync(x => x.UserId == uid && situationIds.Contains(x.ItemSituationId));
            }

            return await dbContext.Item.CountAsync(x => x.UserId == uid);
        }

        public async Task<List<Item>?> GetAsync(int uid, int page, int pageSize, int[]? situationIds)
        {
            if (situationIds is not null && situationIds?.Length > 0)
            {
                return await dbContext.Item.AsNoTracking().Where(x => x.UserId == uid && situationIds.Contains(x.ItemSituationId))
                    .Include(x => x.Category)
                    .Include(x => x.SubCategory)
                    .Include(x => x.ItemSituation)
                    .Include(x => x.AcquisitionType)
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((page - 1) * pageSize).Take(pageSize)
                    .ToListAsync();
            }

            return await dbContext.Item.AsNoTracking().Where(x => x.UserId == uid)
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.ItemSituation)
            .Include(x => x.AcquisitionType)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();
        }
    }
}
