using BookshelfModels;

namespace BookshelfDAL
{
    public interface IBookHistoricDAL
    {
        Task<int> ExecuteAddBookHistoricAsync(BookHistoric bookHistoric);

        Task<int> ExecuteAddRangeBookHistoricItemListAsync(List<BookHistoricItem> bookHistoricItemList);

        List<BookHistoric> ExecuteQueryByCreatedAt(DateTime createdAt, int uid);

        List<BookHistoric> ExecuteQueryByBookId(int Bookid, int uid);
    }
}