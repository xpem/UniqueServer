using BaseModels;
using BookshelfModels;

namespace BookshelfServices
{
    public interface IBookHistoricService
    {
        Task<BookHistoric> BuildAndCreateBookUpdateHistoricAsync(Book oldBook, Book book);

        Task<int> AddBookHistoricAsync(BookHistoric bookHistoric);
        Task<BaseResponse> GetByBookIdAsync(int? bookId, int uid);
        Task<BaseResponse> GetByCreatedAtAsync(DateTime? createdAt, int page, int uid);
    }
}