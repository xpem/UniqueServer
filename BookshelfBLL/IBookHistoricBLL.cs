using BaseModels;
using BookshelfModels;

namespace BookshelfBLL
{
    public interface IBookHistoricBLL
    {
        Task<BookHistoric> BuildAndCreateBookUpdateHistoricAsync(Book oldBook, Book book);

        BaseResponse GetByBookIdOrCreatedAt(int? BookId, DateTime? createdAt, int uid);

        Task<int> AddBookHistoricAsync(BookHistoric bookHistoric);
    }
}