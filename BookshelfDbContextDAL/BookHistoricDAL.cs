using BookshelfDbContextDAL;
using BookshelfModels;

namespace BookshelfDAL
{
    public class BookHistoricDAL : IBookHistoricDAL
    {
        private readonly BookshelfDbContext bookshelfDbContext;

        public BookHistoricDAL(BookshelfDbContext bookshelfDbContext)
        {
            this.bookshelfDbContext = bookshelfDbContext;
        }

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
    }
}
