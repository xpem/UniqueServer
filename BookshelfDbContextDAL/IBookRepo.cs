using BookshelfModels;

namespace BookshelfDbContextDAL
{
    public interface IBookRepo
    {
        Task<int> ExecuteInactivateBookAsync(int bookId, int userId);

        Task<int> UpdateAsync(Book book);

        Task<int> ExecuteCreateBookAsync(Book book);

        Task<Book?> GetBookByTitleWithNotEqualIdAsync(string title, int uid, int bookId);

        Task<Book?> GetBookByTitleAsync(string title, int uid);

        Task<Book?> GetBookByIdAsync(int bookId, int uid);

        Task<List<Book>> GetBooksAfterUpdatedAtAsync(DateTime updatedAt, int page, int pageSize, int uid);
    }
}