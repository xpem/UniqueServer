using BookshelfDbContextDAL;
using BookshelfModels;
using Microsoft.EntityFrameworkCore;

namespace BookshelfDAL
{
    public class BookHistoricRepo(BookshelfDbContext bookshelfDbContext) : IBookHistoricRepo
    {
        public async Task<int> ExecuteAddBookHistoricAsync(BookHistoric bookHistoric)
        {
            await bookshelfDbContext.BookHistoric.AddAsync(bookHistoric);

            return await bookshelfDbContext.SaveChangesAsync();
        }

        public async Task<int> ExecuteAddRangeBookHistoricItemListAsync(List<BookHistoricItem> bookHistoricItemList)
        {
            await bookshelfDbContext.BookHistoricItem.AddRangeAsync(bookHistoricItemList);

            return await bookshelfDbContext.SaveChangesAsync();
        }

        public async Task<List<BookHistoric>> ExecuteQueryByCreatedAtAsync(DateTime createdAt, int page, int pageSize, int uid)
        {
            return await bookshelfDbContext.BookHistoric.Where(x => x.UserId == uid && x.CreatedAt > createdAt)
                                .Include(x => x.BookHistoricItems)
                                .ThenInclude(bhi => bhi.BookHistoricItemField)
                                .Include(x => x.BookHistoricType)
                                .OrderBy(x => x.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<List<BookHistoric>> ExecuteQueryByBookId(int Bookid, int uid)
        {
            return await bookshelfDbContext.BookHistoric.Where(x => x.UserId == uid && x.BookId == Bookid)
                                .Include(x => x.BookHistoricItems)
                                .ThenInclude(bhi => bhi.BookHistoricItemField)
                                .Include(x => x.BookHistoricType)
                                .OrderByDescending(x => x.CreatedAt).ToListAsync();
        }
    }
}
