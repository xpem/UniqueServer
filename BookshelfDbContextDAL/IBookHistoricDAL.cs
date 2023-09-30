using BookshelfModels;

namespace BookshelfDAL
{
    public interface IBookHistoricDAL
    {
        Task<int> ExecuteAddBookHistoricAsync(BookHistoric bookHistoric);

        Task<int> ExecuteAddRangeBookHistoricItemListAsync(List<BookHistoricItem> bookHistoricItemList);
    }
}