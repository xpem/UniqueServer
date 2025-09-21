using InventoryModels.DTOs;
using InventoryModels.Req;
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

        public async Task<int> GetTotalAsync(int uid)
        {
            return await dbContext.Item.CountAsync(x => x.UserId == uid);
        }

        public async Task<int> GetTotalBySearchAsync(int uid, ReqSearchItem reqSearchItem)
        {
            var query = dbContext.Item.AsNoTracking().Where(x => x.UserId == uid);

            if (reqSearchItem is not null)
            {
                if (reqSearchItem.Situations?.Length > 0)
                {
                    query = query.Where(x => reqSearchItem.Situations.Contains(x.ItemSituationId));
                }

                if (!string.IsNullOrWhiteSpace(reqSearchItem.Name))
                {
                    query = query.Where(x => EF.Functions.Like(x.Name.ToLower(), $"%{reqSearchItem.Name.ToLower()}%"));
                }
            }

            return await query.CountAsync();
        }

        public async Task<List<Item>?> GetAsync(int uid, int page, int pageSize)
        {
            return await dbContext.Item.AsNoTracking().Where(x => x.UserId == uid)
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.ItemSituation)
            .Include(x => x.AcquisitionType)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();
        }

        public async Task<List<Item>?> GetBySearchAsync(int uid, int page, int pageSize, ReqSearchItem reqSearchItem)
        {
            var query = dbContext.Item.AsNoTracking().Where(x => x.UserId == uid);

            if (reqSearchItem is not null)
            {
                if (reqSearchItem.Situations is not null && reqSearchItem.Situations.Length > 0)
                {
                    query = query.Where(x => reqSearchItem.Situations.Contains(x.ItemSituationId));
                }

                if (!string.IsNullOrWhiteSpace(reqSearchItem.Name))
                {
                    query = query.Where(x => EF.Functions.Like(x.Name.ToLower(), $"%{reqSearchItem.Name.ToLower()}%"));
                }
            }

            query = query
                  .Include(x => x.Category)
                  .Include(x => x.SubCategory)
                  .Include(x => x.ItemSituation)
                  .Include(x => x.AcquisitionType);

            if (reqSearchItem is not null)
            {
                query = reqSearchItem.OrderBy switch
                {
                    ResultOrderBy.Name => query.OrderBy(x => x.Name).ThenByDescending(x => x.CreatedAt),
                    ResultOrderBy.AcquisitionDate => query.OrderByDescending(x => x.AcquisitionDate).ThenByDescending(x => x.CreatedAt),
                    ResultOrderBy.UpdatedAt => query.OrderByDescending(x => x.UpdatedAt).ThenByDescending(x => x.CreatedAt),
                    _ => query.OrderByDescending(x => x.CreatedAt),
                };
            }

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<string>> GetLastPurchaseStores(int uid, int count)
        {
            return await dbContext.Item.AsNoTracking()
                .Where(x => x.UserId == uid && !string.IsNullOrEmpty(x.PurchaseStore))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.PurchaseStore!)
                .Distinct()
                .Take(count)
                .ToListAsync();
        }
    }
}
