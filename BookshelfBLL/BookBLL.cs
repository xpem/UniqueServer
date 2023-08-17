using BaseModels;
using BookshelfDbContextDAL;
using BookshelfModels;
using BookshelfModels.Request;
using Microsoft.EntityFrameworkCore;
using System;
using UserModels;

namespace BookshelfBLL
{
    public class BookBLL : IBookBLL
    {
        private readonly BookshelfDbContext bookshelfDbContext;

        public BookBLL(BookshelfDbContext bookshelfDbContext)
        {
            this.bookshelfDbContext = bookshelfDbContext;
        }

        public async Task<BLLResponse> CreateBook(ReqBook reqBook, int uid)
        {
            string? validateError = reqBook.ValidateBook();

            if (!string.IsNullOrEmpty(validateError)) return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = validateError } };

            BookshelfModels.Book book = new()
            {
                Cover = reqBook.Cover,
                Title = reqBook.Title,
                Subtitle = reqBook.Subtitle,
                Authors = reqBook.Authors,
                Volume = reqBook.Volume,
                Pages = reqBook.Pages,
                Year = reqBook.Year,
                Status = reqBook.Status,
                Score = reqBook.Score,
                Comment = reqBook.Comment,
                Genre = reqBook.Genre,
                Isbn = reqBook.Isbn,
                GoogleId = reqBook.GoogleId,
                Inactive = reqBook.Inactive,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                UserId = uid,
            };

            string? existingBookMessage = ValidateExistingBook(book);

            if (existingBookMessage != null)
            {
                return new BLLResponse()
                {
                    Content = null,
                    Error = new ErrorMessage() { Error = existingBookMessage }
                };
            }

            await bookshelfDbContext.Book.AddAsync(book);

            BookHistoric bookHistoric = new()
            {
                BookId = book.Id,
                BookHistoricTypeId = 1,
                UserId = book.UserId,
                CreatedAt = DateTime.Now
            };

            await bookshelfDbContext.BookHistoric.AddAsync(bookHistoric);

            await bookshelfDbContext.SaveChangesAsync();

            BookshelfModels.Response.ResBook resBook = new()
            {
                Id = book.Id,
                Title = book.Title,
                Subtitle = book.Subtitle,
                Authors = book.Authors,
                Volume = book.Volume,
                Pages = book.Pages,
                Year = book.Year,
                Status = book.Status,
                Genre = book.Genre,
                Isbn = book.Isbn,
                Cover = book.Cover,
                GoogleId = book.GoogleId,
                Score = book.Score,
                Comment = book.Comment,
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt,
                Inactive = book.Inactive
            };

            return new BLLResponse { Content = resBook, Error = null };
        }


        public async Task<BLLResponse> UpdateBook(ReqBook reqBook, int bookId, int uid)
        {
            if (bookId < 0) 
                return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = "Invalid Book id" } };

            string? validateError = reqBook.ValidateBook();

            if (!string.IsNullOrEmpty(validateError)) 
                return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = validateError } };

            if (bookshelfDbContext.Book.FirstOrDefault(x => x.Title == reqBook.Title && x.UserId == uid && x.Inactive == false && x.Id != bookId) != null)
                return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = "Already exist a book with this title" } };

            Book? book = bookshelfDbContext.Book.FirstOrDefault(x => x.Id == bookId && x.UserId == uid);

            if (book == null)
                return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = "Invalid Book id" } };

            book.Title = reqBook.Title;
            book.Subtitle = reqBook.Subtitle;
            book.Authors = reqBook.Authors;
            book.Volume = reqBook.Volume;
            book.Pages = reqBook.Pages;
            book.Year = reqBook.Year;
            book.Status = reqBook.Status;
            book.Genre = reqBook.Genre;
            book.Isbn = reqBook.Isbn;
            book.Cover = reqBook.Cover;
            book.GoogleId = reqBook.GoogleId;
            book.Score = reqBook.Score;
            book.Comment = reqBook.Comment;
            book.Inactive = reqBook.Inactive;

            bookshelfDbContext.Book.Update(book);

            await bookshelfDbContext.SaveChangesAsync();

            //alternativa
            //await bookshelfDbContext.Book.Where(a => a.Id == bookId).ExecuteUpdateAsync(
            //     b => b.SetProperty(c => c.Title, reqBook.Title)
            //     .SetProperty(c => c.Subtitle, reqBook.Subtitle)
            //     );


            BookshelfModels.Response.ResBook resBook = new()
            {
                Id = book.Id,
                Title = book.Title,
                Subtitle = book.Subtitle,
                Authors = book.Authors,
                Volume = book.Volume,
                Pages = book.Pages,
                Year = book.Year,
                Status = book.Status,
                Genre = book.Genre,
                Isbn = book.Isbn,
                Cover = book.Cover,
                GoogleId = book.GoogleId,
                Score = book.Score,
                Comment = book.Comment,
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt,
                Inactive = book.Inactive
            };

            return new BLLResponse { Content = resBook, Error = null };
        }

        protected string? ValidateExistingBook(BookshelfModels.Book book)
        {
            Book? bookResp = bookshelfDbContext.Book.FirstOrDefault(x => x.Title.Equals(book.Title) && x.UserId.Equals(book.UserId) && x.Inactive.Equals(false));

            if (bookResp != null)
                return "A book with this title has already been added";

            return null;
        }

    }
}