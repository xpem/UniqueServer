using BookshelfModels;

namespace BookshelfRepo
{
    public interface IBookHistoricRepo
    {
        Task<int> AddAsync(BookHistoric bookHistoric);

        Task<int> AddRangeItemListAsync(List<BookHistoricItem> bookHistoricItemList);

        Task<List<BookHistoric>> GetByCreatedAtAsync(DateTime createdAt, int page, int pageSize, int uid);

        Task<List<BookHistoric>> GetByBookId(int Bookid, int uid);

        Task<int> DeleteAllAsync(int uid);
    }
}