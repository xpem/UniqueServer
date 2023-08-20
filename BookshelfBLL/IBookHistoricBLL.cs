using BaseModels;
using BookshelfModels;

namespace BookshelfBLL
{
    public interface IBookHistoricBLL
    {
        Task<BookHistoric> BuildAndCreateBookUpdateHistoric(Book oldBook, Book book);

        BLLResponse GetByBookIdOrCreatedAt(int? BookId, DateTime? createdAt, int uid);
    }
}