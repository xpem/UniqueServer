using BaseModels;
using BookshelfModels;

namespace BookshelfServices
{
    public interface IBookHistoricService
    {
        Task<BookHistoric> BuildAndCreateBookUpdateHistoricAsync(Book oldBook, Book book);

        Task<int> AddAsync(BookHistoric bookHistoric);
        Task<BaseResp> GetByBookIdAsync(int? bookId, int uid);
        Task<BaseResp> GetByCreatedAtAsync(DateTime? createdAt, int page, int uid);
        Task<int> DeleteAllAsync(int uid);
    }
}