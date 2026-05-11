using InventoryModels.DTOs;
using InventoryModels.Req;
using InventoryModels.Res.Item;
using InventoryRepos.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryRepos
{
    public class ItemRepo(IDbContextFactory<InventoryDbCtx> dbCtx) : IItemRepo
    {
        public async Task<Item?> GetById(int uid, int id)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.Item.Where(x => x.Id == id && x.UserId == uid)
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .Include(x => x.ItemSituation)
                .Include(x => x.AcquisitionType)
                .FirstOrDefaultAsync();
        }

        public int Create(Item item)
        {
            using var context = dbCtx.CreateDbContext();
            context.ChangeTracker?.Clear();
            context.Item.Add(item);
            return context.SaveChanges();
        }

        public int Update(Item item)
        {
            using var context = dbCtx.CreateDbContext();
            context.ChangeTracker?.Clear();
            context.Item.Update(item);
            return context.SaveChanges();
        }

        public int UpdateFileNames(int uid, int id, string? fileName1, string? fileName2)
        {
            using var context = dbCtx.CreateDbContext();
            context.ChangeTracker?.Clear();

            return context.Item.Where(x => x.UserId == uid && x.Id == id).ExecuteUpdate(y => y
               .SetProperty(z => z.Image1, fileName1)
               .SetProperty(z => z.Image2, fileName2)
               .SetProperty(z => z.UpdatedAt, DateTime.Now)
               );
        }

        public async Task<bool> CheckItemImageNameAsync(int uid, int id, string imageName)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.Item.AnyAsync(x => x.Id == id && x.UserId == uid && (x.Image1 == imageName || x.Image2 == imageName));
        }

        public int Delete(Item item)
        {
            using var context = dbCtx.CreateDbContext();
            context.ChangeTracker?.Clear();
            context.Item.Remove(item);
            return context.SaveChanges();
        }

        public async Task<int> GetTotalAsync(int uid)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.Item.CountAsync(x => x.UserId == uid);
        }

        public async Task<int> GetTotalBySearchAsync(int uid, ReqSearchItem reqSearchItem)
        {
            using var context = dbCtx.CreateDbContext();
            var query = context.Item.AsNoTracking().Where(x => x.UserId == uid);

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
            using var context = dbCtx.CreateDbContext();
            return await context.Item.AsNoTracking().Where(x => x.UserId == uid)
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
            using var context = dbCtx.CreateDbContext();
            var query = context.Item.AsNoTracking().Where(x => x.UserId == uid);

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
            using var context = dbCtx.CreateDbContext();
            return await context.Item.AsNoTracking()
                .Where(x => x.UserId == uid && !string.IsNullOrEmpty(x.PurchaseStore))
                .OrderByDescending(x => x.CreatedAt)
                .ThenBy(x => x.PurchaseStore)
                .Select(x => x.PurchaseStore!)
                .Distinct()
                .Take(count)                                
                .ToListAsync();
        }

        public async Task<List<ResItemSituationsGroupingWithQuantities>> GetItemSituationsWithQuantities(int uid)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.Item
                .AsNoTracking()
                .Where(i => i.UserId == uid)
                .Join(
                    context.ItemSituation,
                    i => i.ItemSituationId,
                    t => t.Id,
                    (i, t) => new { i.ItemSituationId, t.Name }
                )
                .GroupBy(x => new { x.ItemSituationId, x.Name })
                .Select(g => new ResItemSituationsGroupingWithQuantities(
                    g.Key.ItemSituationId,
                    g.Key.Name,
                    g.Count()
                ))
                .ToListAsync();
        }
    }
}
