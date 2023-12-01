using BookshelfDbContextDAL;
using BookshelfModels;
using Microsoft.EntityFrameworkCore;

namespace BookshelfDAL
{
    public class BookHistoricDAL(BookshelfDbContext bookshelfDbContext) : IBookHistoricDAL
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

        public List<BookHistoric> ExecuteQueryByCreatedAt(DateTime createdAt, int uid)
        {
#pragma warning disable CS8620 // O argumento não pode ser usado para o parâmetro devido a diferenças na nulidade dos tipos de referência.
            return
            [
                .. bookshelfDbContext.BookHistoric.Where(x => x.UserId == uid && x.CreatedAt > createdAt)
                                .Include(x => x.BookHistoricItems)
                                .ThenInclude(bhi => bhi.BookHistoricItemField)
                                .Include(x => x.BookHistoricType)
                                .OrderByDescending(x => x.CreatedAt),
            ];
#pragma warning restore CS8620 // O argumento não pode ser usado para o parâmetro devido a diferenças na nulidade dos tipos de referência.
        }

        public List<BookHistoric> ExecuteQueryByBookId(int Bookid, int uid)
        {
#pragma warning disable CS8620 // O argumento não pode ser usado para o parâmetro devido a diferenças na nulidade dos tipos de referência.
            return
            [
                .. bookshelfDbContext.BookHistoric.Where(x => x.UserId == uid && x.BookId == Bookid)
                                .Include(x => x.BookHistoricItems)
                                .ThenInclude(bhi => bhi.BookHistoricItemField)
                                .Include(x => x.BookHistoricType)
                                .OrderByDescending(x => x.CreatedAt),
            ];
#pragma warning restore CS8620 // O argumento não pode ser usado para o parâmetro devido a diferenças na nulidade dos tipos de referência.
        }
    }
}
