﻿using BaseModels;
using BookshelfModels;
using BookshelfModels.Request;
using BookshelfModels.Response;
using BookshelfRepo;

namespace BookshelfServices
{
    public class BookService(IBookHistoricService bookHistoricBLL, IBookRepo bookRepo) : IBookService
    {
        readonly int pageSize = 50;

        public async Task<BaseResponse> CreateAsync(ReqBook reqBook, int uid)
        {
            //BookshelfDbContextDAL.BookshelfInitializeDB ini = new BookshelfInitializeDB(bookshelfDbContext);
            //ini.CreateInitialValues();

            string? validateError = reqBook.Validate();

            if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(null, validateError);

            Book book = new()
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
                Inactive = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                UserId = uid,
            };

            string? existingBookMessage = await ValidateExistingBookAsync(book);

            if (existingBookMessage != null)
                return new BaseResponse(null, existingBookMessage);

            await bookRepo.CreateAsync(book);

            BookHistoric bookHistoric = new()
            {
                BookId = book.Id,
                BookHistoricTypeId = 1,
                UserId = book.UserId,
                CreatedAt = DateTime.Now
            };

            await bookHistoricBLL.AddAsync(bookHistoric);

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

            return new BaseResponse(resBook);
        }

        public async Task<BaseResponse> UpdateAsync(ReqBook reqBook, int bookId, int uid)
        {
            string? validateError = reqBook.Validate();

            if (!string.IsNullOrEmpty(validateError))
                return new BaseResponse(null, validateError);

            if ((await bookRepo.GetBookByTitleWithNotEqualIdAsync(reqBook.Title, uid, bookId) != null))
                return new BaseResponse(null, "Already exist a book with this title");

            Book? oldBook = await bookRepo.GetBookByIdAsync(bookId, uid);

            if (oldBook == null)
                return new BaseResponse(null, "Invalid Book id");

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
                Inactive = oldBook.Inactive,
                CreatedAt = oldBook.CreatedAt,
                UserId = oldBook.UserId,
                UpdatedAt = oldBook.UpdatedAt,
                Id = oldBook.Id
            };

            if (NewBookHasChanges(oldBook, newBook))
            {
                newBook.UpdatedAt = DateTime.Now;

                await bookRepo.UpdateAsync(newBook);

                //alternativa
                //await bookshelfDbContext.Book.Where(a => a.Id == bookId).ExecuteUpdateAsync(
                //     b => b.SetProperty(c => c.Title, reqBook.Title)
                //     .SetProperty(c => c.Subtitle, reqBook.Subtitle)
                //     );

                await bookHistoricBLL.BuildAndCreateBookUpdateHistoricAsync(oldBook, newBook);
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

            return new BaseResponse(resBook);
        }

        public async Task<int> DeleteAllAsync(int uid) => await bookRepo.DeleteAllAsync(uid);

        public async Task<BaseResponse> InactivateAsync(int bookId, int uid)
        {
            if (bookId < 0)
                return new BaseResponse(null, "Invalid Book id");

            Book? oldBook = await bookRepo.GetBookByIdAsync(bookId, uid);

            if (oldBook == null)
                return new BaseResponse(null, "Invalid Book id");

            if (oldBook.Inactive == false)
            {
                await bookRepo.InactivateAsync(bookId, uid);

                BookHistoric bookHistoric = new()
                {
                    BookId = oldBook.Id,
                    BookHistoricTypeId = 4,
                    UserId = oldBook.UserId,
                    CreatedAt = DateTime.Now
                };

                await bookHistoricBLL.AddAsync(bookHistoric);
            }

            return new BaseResponse(true);
        }

        public async Task<BaseResponse> GetByUpdatedAtAsync(DateTime updatedAt, int page, int uid)
        {
            if (page <= 0)
                return new BaseResponse(null, "Invalid page");

            List<Book> books = await bookRepo.GetBooksAfterUpdatedAtAsync(updatedAt, page, pageSize, uid);

            List<ResBook> resBooks = [];

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

            return new BaseResponse(resBooks);
        }

        protected async Task<string?> ValidateExistingBookAsync(Book book)
        {
            Book? bookResp = await bookRepo.GetBookByTitleAsync(book.Title, book.UserId);

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

            if (oldBook.Comment != newBook.Comment)
                return true;

            if (oldBook.Genre != newBook.Genre)
                return true;

            if (oldBook.Isbn != newBook.Isbn)
                return true;

            return false;
        }
    }
}