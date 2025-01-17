﻿using BookshelfModels;
using Microsoft.EntityFrameworkCore;

namespace BookshelfRepo
{
    public class BookRepo(BookshelfDbContext bookshelfDbContext) : IBookRepo
    {
        public async Task<Book?> GetBookByTitleWithNotEqualIdAsync(string title, int uid, int bookId) =>
            await bookshelfDbContext.Book.FirstOrDefaultAsync(x => x.Title == title && x.UserId == uid && x.Inactive == false && x.Id != bookId);

        public async Task<Book?> GetBookByTitleAsync(string title, int uid) =>
            await bookshelfDbContext.Book.FirstOrDefaultAsync(x => x.Title == title && x.UserId == uid && x.Inactive == false);

        public async Task<Book?> GetBookByIdAsync(int bookId, int uid) =>
            await bookshelfDbContext.Book.FirstOrDefaultAsync(x => x.Id == bookId && x.UserId == uid);

        public async Task<List<Book>> GetBooksAfterUpdatedAtAsync(DateTime updatedAt, int page, int pageSize, int uid) =>
            await bookshelfDbContext.Book.Where(x => x.UpdatedAt > updatedAt && x.UserId == uid).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        public async Task<int> InactivateAsync(int bookId, int userId)
        {
            bookshelfDbContext.ChangeTracker?.Clear();

            bookshelfDbContext.Book.Where(x => x.UserId == userId && x.Id == bookId).ExecuteUpdate(
                y => y.SetProperty(z => z.Inactive, true)
                .SetProperty(z => z.UpdatedAt, DateTime.Now));

            return await bookshelfDbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(Book book)
        {
            bookshelfDbContext.ChangeTracker?.Clear();

            bookshelfDbContext.Book.Update(book);

            return await bookshelfDbContext.SaveChangesAsync();
        }

        public async Task<int> CreateAsync(Book book)
        {
            await bookshelfDbContext.Book.AddAsync(book);

            return await bookshelfDbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAllAsync(int uid) => 
            await bookshelfDbContext.Book.Where(x => x.UserId == uid).ExecuteDeleteAsync();
    }
}
