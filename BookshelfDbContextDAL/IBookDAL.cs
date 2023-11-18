using BaseModels;
using BookshelfModels;
using BookshelfModels.Request;

namespace BookshelfDbContextDAL
{
    public interface IBookDAL
    {
        Task<int> ExecuteInactivateBookAsync(int bookId, int userId);

        Task<int> ExecuteUpdateBookAsync(Book book);

        Task<int> ExecuteAddBookAsync(Book book);

        Task<Book?> GetBookByTitleWithNotEqualIdAsync(string title, int uid, int bookId);

        Task<Book?> GetBookByTitleAsync(string title, int uid);

        Task<Book?> GetBookByIdAsync(int bookId, int uid);

        IQueryable<Book> GetBooksAfterUpdatedAt(DateTime updatedAt, int uid);
    }
}