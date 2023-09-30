using BookshelfModels;
using BookshelfModels.Request;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BookshelfDbContextDAL
{
    public class BookDAL : IBookDAL
    {
        private readonly BookshelfDbContext bookshelfDbContext;

        public BookDAL(BookshelfDbContext bookshelfDbContext)
        {
            this.bookshelfDbContext = bookshelfDbContext;
        }

        public async Task<Book?> GetBookByTitleWithNotEqualIdAsync(string title, int uid, int bookId) => await bookshelfDbContext.Book.FirstOrDefaultAsync(x => x.Title == title && x.UserId == uid && x.Inactive == false && x.Id != bookId);

        public async Task<Book?> GetBookByTitleAsync(string title, int uid) => await bookshelfDbContext.Book.FirstOrDefaultAsync(x => x.Title == title && x.UserId == uid && x.Inactive == false);

        public async Task<Book?> GetBookByIdAsync(int bookId, int uid) => await bookshelfDbContext.Book.FirstOrDefaultAsync(x => x.Id == bookId && x.UserId == uid);

        public IQueryable<Book> GetBooksAfterUpdatedAt(DateTime updatedAt, int uid) => bookshelfDbContext.Book.Where(x => x.UpdatedAt > updatedAt && x.UserId == uid);

        public async Task<int> ExecuteInactivateBookAsync(int bookId, int userId)
        {
            bookshelfDbContext.ChangeTracker?.Clear();

            bookshelfDbContext.Book.Where(x => x.UserId == userId && x.Id == bookId).ExecuteUpdate(
                y => y.SetProperty(z => z.Inactive, true)
                .SetProperty(z => z.UpdatedAt, DateTime.Now));

            return await bookshelfDbContext.SaveChangesAsync();
        }

        public async Task<int> ExecuteUpdateBookAsync(Book book)
        {
            bookshelfDbContext.ChangeTracker?.Clear();

            bookshelfDbContext.Book.Update(book);

            return await bookshelfDbContext.SaveChangesAsync();
        }

        public async Task<int> ExecuteAddBookAsync(Book book)
        {
            await bookshelfDbContext.Book.AddAsync(book);

            return await bookshelfDbContext.SaveChangesAsync();
        }
    }
}
