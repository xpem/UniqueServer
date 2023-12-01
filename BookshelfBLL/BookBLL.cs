using BaseModels;
using BookshelfDbContextDAL;
using BookshelfModels;
using BookshelfModels.Request;
using BookshelfModels.Response;
using System.Net;

namespace BookshelfBLL
{
    public class BookBLL(IBookHistoricBLL bookHistoricBLL, IBookDAL bookDAL) : IBookBLL
    {
        public async Task<BLLResponse> CreateBook(ReqBook reqBook, int uid)
        {
            //BookshelfDbContextDAL.BookshelfInitializeDB ini = new BookshelfInitializeDB(bookshelfDbContext);
            //ini.CreateInitialValues();

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
                Inactive = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                UserId = uid,
            };

            string? existingBookMessage = await ValidateExistingBookAsync(book);

            if (existingBookMessage != null)
            {
                return new BLLResponse()
                {
                    Content = null,
                    Error = new ErrorMessage() { Error = existingBookMessage }
                };
            }

            await bookDAL.ExecuteAddBookAsync(book);

            BookHistoric bookHistoric = new()
            {
                BookId = book.Id,
                BookHistoricTypeId = 1,
                UserId = book.UserId,
                CreatedAt = DateTime.Now
            };

            await bookHistoricBLL.AddBookHistoricAsync(bookHistoric);

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

            if ((await bookDAL.GetBookByTitleWithNotEqualIdAsync(reqBook.Title, uid, bookId) != null))
                return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = "Already exist a book with this title" } };

            Book? oldBook = await bookDAL.GetBookByIdAsync(bookId, uid);

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
                Inactive = oldBook.Inactive,
                CreatedAt = oldBook.CreatedAt,
                UserId = oldBook.UserId,
                UpdatedAt = oldBook.UpdatedAt,
                Id = oldBook.Id
            };

            if (NewBookHasChanges(oldBook, newBook))
            {
                newBook.UpdatedAt = DateTime.Now;

                await bookDAL.ExecuteUpdateBookAsync(newBook);

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

            return new BLLResponse { Content = resBook, Error = null };
        }

        public async Task<BLLResponse> InactivateBook(int bookId, int uid)
        {
            if (bookId < 0)
                return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = "Invalid Book id" } };

            Book? oldBook = await bookDAL.GetBookByIdAsync(bookId, uid);

            if (oldBook == null)
                return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = "Invalid Book id" } };

            if (oldBook.Inactive == false)
            {
                await bookDAL.ExecuteInactivateBookAsync(bookId, uid);

                BookHistoric bookHistoric = new()
                {
                    BookId = oldBook.Id,
                    BookHistoricTypeId = 4,
                    UserId = oldBook.UserId,
                    CreatedAt = DateTime.Now
                };

                await bookHistoricBLL.AddBookHistoricAsync(bookHistoric);
            }

            return new BLLResponse { Content = true, Error = null };
        }

        public BLLResponse GetByUpdatedAt(DateTime updatedAt, int uid)
        {
            //string? validateError = req.Validate();

            //if (!string.IsNullOrEmpty(validateError))
            //    return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = validateError } };

            IQueryable<Book> books = bookDAL.GetBooksAfterUpdatedAt(updatedAt, uid);

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

            return new BLLResponse() { Content = resBooks, Error = null };
        }

        protected async Task<string?> ValidateExistingBookAsync(Book book)
        {
            Book? bookResp = await bookDAL.GetBookByTitleAsync(book.Title, book.UserId);

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