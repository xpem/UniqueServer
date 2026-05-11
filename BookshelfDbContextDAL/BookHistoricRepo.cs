using BookshelfModels;
using Microsoft.EntityFrameworkCore;

namespace BookshelfRepo
{
    public class BookHistoricRepo(IDbContextFactory<BookshelfDbCtx> dbCtx) : IBookHistoricRepo
    {
        public async Task<int> AddAsync(BookHistoric bookHistoric)
        {
            using var context = dbCtx.CreateDbContext();
            await context.BookHistoric.AddAsync(bookHistoric);

            return await context.SaveChangesAsync();
        }

        public async Task<int> AddRangeItemListAsync(List<BookHistoricItem> bookHistoricItemList)
        {
            using var context = dbCtx.CreateDbContext();
            await context.BookHistoricItem.AddRangeAsync(bookHistoricItemList);

            return await context.SaveChangesAsync();
        }

        public async Task<List<BookHistoric>> GetByCreatedAtAsync(DateTime createdAt, int page, int pageSize, int uid)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.BookHistoric.Where(x => x.UserId == uid && x.CreatedAt > createdAt)
                                .Include(x => x.BookHistoricItems)
                                .ThenInclude(bhi => bhi.BookHistoricItemField)
                                .Include(x => x.BookHistoricType)
                                .OrderBy(x => x.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<List<BookHistoric>> GetByBookId(int Bookid, int uid)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.BookHistoric.Where(x => x.UserId == uid && x.BookId == Bookid)
                                .Include(x => x.BookHistoricItems)
                                .ThenInclude(bhi => bhi.BookHistoricItemField)
                                .Include(x => x.BookHistoricType)
                                .OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// delete historic with all relationated items
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public async Task<int> DeleteAllAsync(int uid)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.BookHistoric.Where(x => x.UserId == uid).Include(x => x.BookHistoricItems).ExecuteDeleteAsync();
        }
    }
}
