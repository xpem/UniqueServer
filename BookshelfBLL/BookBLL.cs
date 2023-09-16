﻿using BaseModels;
using BookshelfDbContextDAL;
using BookshelfModels;
using BookshelfModels.Request;
using BookshelfModels.Response;

namespace BookshelfBLL
{
    public class BookBLL : IBookBLL
    {
        private readonly BookshelfDbContext bookshelfDbContext;
        private readonly IBookHistoricBLL bookHistoricBLL;

        public BookBLL(BookshelfDbContext bookshelfDbContext, IBookHistoricBLL bookHistoricBLL)
        {
            this.bookshelfDbContext = bookshelfDbContext;
            this.bookHistoricBLL = bookHistoricBLL;

            //BookshelfDbContextDAL.BookshelfInitializeDB ini = new BookshelfInitializeDB(bookshelfDbContext);
            //ini.CreateInitialValues();
        }

        public async Task<BLLResponse> CreateBook(ReqBook reqBook, int uid)
        {

            string? validateError = reqBook.Validate();

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

            await bookshelfDbContext.SaveChangesAsync();

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

            string? validateError = reqBook.Validate();

            if (!string.IsNullOrEmpty(validateError))
                return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = validateError } };

            if (bookshelfDbContext.Book.FirstOrDefault(x => x.Title == reqBook.Title && x.UserId == uid && x.Inactive == false && x.Id != bookId) != null)
                return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = "Already exist a book with this title" } };

            Book? oldBook = bookshelfDbContext.Book.FirstOrDefault(x => x.Id == bookId && x.UserId == uid);

            if (oldBook == null)
                return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = "Invalid Book id" } };

            Book? newBook = new()
            {
                Title = reqBook.Title,
                Subtitle = reqBook.Subtitle,
                Authors = reqBook.Authors,
                Volume = reqBook.Volume,
                Pages = reqBook.Pages,
                Year = reqBook.Year,
                Status = reqBook.Status,
                Genre = reqBook.Genre,
                Isbn = reqBook.Isbn,
                Cover = reqBook.Cover,
                GoogleId = reqBook.GoogleId,
                Score = reqBook.Score,
                Comment = reqBook.Comment,
                Inactive = reqBook.Inactive,
                CreatedAt = oldBook.CreatedAt,
                UserId = oldBook.UserId,
                UpdatedAt = oldBook.UpdatedAt,
                Id = oldBook.Id
            };

            if (NewBookHasChanges(oldBook,newBook))
            {
                newBook.UpdatedAt = DateTime.Now;

                bookshelfDbContext.ChangeTracker?.Clear();

                bookshelfDbContext.Book.Update(newBook);

                await bookshelfDbContext.SaveChangesAsync();

                //alternativa
                //await bookshelfDbContext.Book.Where(a => a.Id == bookId).ExecuteUpdateAsync(
                //     b => b.SetProperty(c => c.Title, reqBook.Title)
                //     .SetProperty(c => c.Subtitle, reqBook.Subtitle)
                //     );

                await bookHistoricBLL.BuildAndCreateBookUpdateHistoric(oldBook, newBook);
            }

            BookshelfModels.Response.ResBook resBook = new()
            {
                Id = newBook.Id,
                Title = newBook.Title,
                Subtitle = newBook.Subtitle,
                Authors = newBook.Authors,
                Volume = newBook.Volume,
                Pages = newBook.Pages,
                Year = newBook.Year,
                Status = newBook.Status,
                Genre = newBook.Genre,
                Isbn = newBook.Isbn,
                Cover = newBook.Cover,
                GoogleId = newBook.GoogleId,
                Score = newBook.Score,
                Comment = newBook.Comment,
                CreatedAt = newBook.CreatedAt,
                UpdatedAt = newBook.UpdatedAt,
                Inactive = newBook.Inactive
            };

            return new BLLResponse { Content = resBook, Error = null };
        }

        public BLLResponse GetByUpdatedAt(DateTime updatedAt, int uid)
        {
            //string? validateError = req.Validate();

            //if (!string.IsNullOrEmpty(validateError))
            //    return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = validateError } };

            IQueryable<Book> books = bookshelfDbContext.Book.Where(x => x.UpdatedAt > updatedAt && x.UserId == uid);

            List<ResBook> resBooks = new();

            foreach (Book? book in books)
            {
                resBooks.Add(new ResBook()
                {
                    Id = book.Id,
                    Cover = book.Cover,
                    Title = book.Title,
                    Subtitle = book.Subtitle,
                    Authors = book.Authors,
                    Volume = book.Volume,
                    Pages = book.Pages,
                    Year = book.Year,
                    Status = book.Status,
                    Score = book.Score,
                    Comment = book.Comment,
                    Genre = book.Genre,
                    Isbn = book.Isbn,
                    GoogleId = book.GoogleId,
                    Inactive = book.Inactive,
                    CreatedAt = book.CreatedAt,
                    UpdatedAt = book.UpdatedAt
                });
            }

            return new BLLResponse() { Content = resBooks, Error = null };
        }

        protected string? ValidateExistingBook(BookshelfModels.Book book)
        {
            Book? bookResp = bookshelfDbContext.Book.FirstOrDefault(x => x.Title.Equals(book.Title) && x.UserId.Equals(book.UserId) && x.Inactive.Equals(false));

            if (bookResp != null)
                return "A book with this title has already been added";

            return null;
        }

        protected static bool NewBookHasChanges(Book oldBook, Book newBook)
        {
            if (oldBook.Title != newBook.Title)
                return true;

            if (oldBook.Subtitle != newBook.Subtitle)
                return true;

            if (oldBook.Authors != newBook.Authors)
                return true;

            if (oldBook.Volume != newBook.Volume)
                return true;

            if (oldBook.Pages != newBook.Pages)
                return true;

            if (oldBook.Year != newBook.Year)
                return true;

            if (oldBook.Status != newBook.Status)
                return true;

            if (oldBook.Score != newBook.Score)
                return true;

            if (oldBook.Genre != newBook.Genre)
                return true;

            if (oldBook.Isbn != newBook.Isbn)
                return true;

            return false;
        }
    }
}