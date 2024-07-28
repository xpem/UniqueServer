using BookshelfModels;

namespace BookshelfDAL
{
    public interface IBookHistoricRepo
    {
        Task<int> ExecuteAddBookHistoricAsync(BookHistoric bookHistoric);

        Task<int> ExecuteAddRangeBookHistoricItemListAsync(List<BookHistoricItem> bookHistoricItemList);

        Task<List<BookHistoric>> ExecuteQueryByCreatedAtAsync(DateTime createdAt, int page, int pageSize, int uid);

        Task<List<BookHistoric>> ExecuteQueryByBookId(int Bookid, int uid);
    }
}