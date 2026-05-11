using BookshelfModels;
using Microsoft.EntityFrameworkCore;

namespace BookshelfRepo
{
    public class BookRepo(IDbContextFactory<BookshelfDbCtx> dbCtx) : IBookRepo
    {
        public async Task<Book?> GetBookByTitleWithNotEqualIdAsync(string title, int uid, int bookId)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.Book.FirstOrDefaultAsync(x => x.Title == title && x.UserId == uid && x.Inactive == false && x.Id != bookId);
        }

        public async Task<Book?> GetBookByTitleAsync(string title, int uid)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.Book.FirstOrDefaultAsync(x => x.Title == title && x.UserId == uid && x.Inactive == false);
        }

        public async Task<Book?> GetBookByIdAsync(int bookId, int uid)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.Book.FirstOrDefaultAsync(x => x.Id == bookId && x.UserId == uid);
        }

        public async Task<List<Book>> GetBooksAfterUpdatedAtAsync(DateTime updatedAt, int page, int pageSize, int uid)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.Book.Where(x => x.UpdatedAt > updatedAt && x.UserId == uid).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<int> InactivateAsync(int bookId, int userId)
        {
            using var context = dbCtx.CreateDbContext();
            context.ChangeTracker?.Clear();

            context.Book.Where(x => x.UserId == userId && x.Id == bookId).ExecuteUpdate(
                y => y.SetProperty(z => z.Inactive, true)
                .SetProperty(z => z.UpdatedAt, DateTime.Now));

            return await context.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(Book book)
        {
            using var context = dbCtx.CreateDbContext();
            context.ChangeTracker?.Clear();

            context.Book.Update(book);

            return await context.SaveChangesAsync();
        }

        public async Task<int> CreateAsync(Book book)
        {
            using var context = dbCtx.CreateDbContext();
            await context.Book.AddAsync(book);

            return await context.SaveChangesAsync();
        }

        public async Task<int> DeleteAllAsync(int uid)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.Book.Where(x => x.UserId == uid).ExecuteDeleteAsync();
        }
    }
}
