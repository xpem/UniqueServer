using BookshelfModels;

namespace BookshelfBLL
{
    public interface IBookHistoricBLL
    {
        Task BuildAndCreateBookUpdateHistoric(Book oldBook, Book book);
    }
}