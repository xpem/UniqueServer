using BaseModels;
using BookshelfDAL;
using BookshelfModels;
using BookshelfModels.Response;

namespace BookshelfServices
{

    public class BookHistoricService(IBookHistoricRepo bookHistoricDAL) : IBookHistoricService
    {
        readonly int pageSize = 50;

        public async Task<BookHistoric> BuildAndCreateBookUpdateHistoricAsync(Book oldBook, Book book)
        {
            List<BookHistoricItem> bookHistoricItemList = [];

            BookHistoric bookHistoric = new()
            {
                BookId = book.Id,
                BookHistoricTypeId = 2,
                UserId = book.UserId,
                CreatedAt = DateTime.Now
            };

            await AddBookHistoricAsync(bookHistoric);

            if (oldBook.Title != book.Title)
                bookHistoricItemList.Add(new BookHistoricItem()
                {
                    BookHistoricItemFieldId = 2,
                    UpdatedFrom = oldBook.Title,
                    UpdatedTo = book.Title,
                    BookHistoricId = bookHistoric.Id,
                    CreatedAt = DateTime.Now
                });

            if (oldBook.Subtitle != book.Subtitle)
                bookHistoricItemList.Add(new BookHistoricItem()
                {
                    BookHistoricItemFieldId = 1,
                    UpdatedFrom = oldBook.Subtitle ?? "",
                    UpdatedTo = book.Subtitle ?? "",
                    BookHistoricId = bookHistoric.Id,
                    CreatedAt = DateTime.Now
                });

            if (oldBook.Authors != book.Authors)
                bookHistoricItemList.Add(new BookHistoricItem()
                {
                    BookHistoricItemFieldId = 4,
                    UpdatedFrom = oldBook.Authors ?? "",
                    UpdatedTo = book.Authors ?? "",
                    BookHistoricId = bookHistoric.Id,
                    CreatedAt = DateTime.Now
                });

            if (oldBook.Volume != book.Volume)
                bookHistoricItemList.Add(new BookHistoricItem()
                {
                    BookHistoricItemFieldId = 5,
                    UpdatedFrom = oldBook.Volume.ToString() ?? "",
                    UpdatedTo = book.Volume.ToString() ?? "",
                    BookHistoricId = bookHistoric.Id,
                    CreatedAt = DateTime.Now
                });

            if (oldBook.Pages != book.Pages)
                bookHistoricItemList.Add(new BookHistoricItem()
                {
                    BookHistoricItemFieldId = 6,
                    UpdatedFrom = oldBook.Pages.ToString() ?? "",
                    UpdatedTo = book.Pages.ToString() ?? "",
                    BookHistoricId = bookHistoric.Id,
                    CreatedAt = DateTime.Now
                });

            if (oldBook.Year != book.Year)
                bookHistoricItemList.Add(new BookHistoricItem()
                {
                    BookHistoricItemFieldId = 7,
                    UpdatedFrom = oldBook.Year.ToString() ?? "",
                    UpdatedTo = book.Year.ToString() ?? "",
                    BookHistoricId = bookHistoric.Id,
                    CreatedAt = DateTime.Now
                });

            if (oldBook.Status != book.Status)
                bookHistoricItemList.Add(new BookHistoricItem()
                {
                    BookHistoricItemFieldId = 8,
                    UpdatedFrom = oldBook.Status.ToString() ?? "",
                    UpdatedTo = book.Status.ToString() ?? "",
                    BookHistoricId = bookHistoric.Id,
                    CreatedAt = DateTime.Now
                });

            if (oldBook.Score != book.Score)
                bookHistoricItemList.Add(new BookHistoricItem()
                {
                    BookHistoricItemFieldId = 9,
                    UpdatedFrom = oldBook.Score.ToString() ?? "",
                    UpdatedTo = book.Score.ToString() ?? "",
                    BookHistoricId = bookHistoric.Id,
                    CreatedAt = DateTime.Now
                });

            if (oldBook.Genre != book.Genre)
                bookHistoricItemList.Add(new BookHistoricItem()
                {
                    BookHistoricItemFieldId = 10,
                    UpdatedFrom = oldBook.Genre ?? "",
                    UpdatedTo = book.Genre ?? "",
                    BookHistoricId = bookHistoric.Id,
                    CreatedAt = DateTime.Now
                });

            if (oldBook.Isbn != book.Isbn)
                bookHistoricItemList.Add(new BookHistoricItem()
                {
                    BookHistoricItemFieldId = 11,
                    UpdatedFrom = oldBook.Isbn ?? "",
                    UpdatedTo = book.Isbn ?? "",
                    BookHistoricId = bookHistoric.Id,
                    CreatedAt = DateTime.Now
                });

            await bookHistoricDAL.AddRangeItemListAsync(bookHistoricItemList);

            bookHistoric.BookHistoricItems = bookHistoricItemList;

            return bookHistoric;
        }

        public async Task<BaseResponse> GetByBookIdAsync(int? bookId, int uid)
        {
            List<BookHistoric> bookHistorics;

            if (bookId is not null)
                bookHistorics = await bookHistoricDAL.GetByBookId(bookId.Value, uid);
            else return new BaseResponse("sem parametro válido de busca");

            var resBookHistorics = BuildBookHistoricList(bookHistorics);

            return new BaseResponse(resBookHistorics);
        }


        public async Task<BaseResponse> GetByCreatedAtAsync(DateTime? createdAt, int page, int uid)
        {
            List<BookHistoric> bookHistorics;

            if (createdAt.HasValue)
                bookHistorics = await bookHistoricDAL.GetByCreatedAtAsync(createdAt.Value, page, pageSize, uid);
            else return new BaseResponse("sem parametro válido de busca");

            var resBookHistorics = BuildBookHistoricList(bookHistorics);

            return new BaseResponse(resBookHistorics);
        }

        private static List<ResBookHistoric> BuildBookHistoricList(List<BookHistoric> bookHistorics)
        {
            List<ResBookHistoric> resBookHistorics = [];

            foreach (BookHistoric? _bookHistoric in bookHistorics)
            {
                List<ResBookHistoricItem> resBookHistoricItems = [];

                if (_bookHistoric.BookHistoricItems != null)
                    foreach (BookHistoricItem _bookHistoricItem in _bookHistoric.BookHistoricItems)
                    {
                        resBookHistoricItems.Add(new ResBookHistoricItem()
                        {
                            Id = _bookHistoricItem.Id,
                            BookFieldId = _bookHistoricItem.BookHistoricItemFieldId,
                            BookFieldName = _bookHistoricItem.BookHistoricItemField?.Name,
                            CreatedAt = _bookHistoricItem.CreatedAt,
                            UpdatedFrom = _bookHistoricItem.UpdatedFrom,
                            UpdatedTo = _bookHistoricItem.UpdatedTo,
                        });
                    }

                resBookHistorics.Add(new ResBookHistoric()
                {
                    Id = _bookHistoric.Id,
                    TypeId = _bookHistoric.BookHistoricType?.Id,
                    TypeName = _bookHistoric.BookHistoricType?.Name,
                    CreatedAt = _bookHistoric.CreatedAt,
                    BookId = _bookHistoric.BookId,
                    BookHistoricItems = resBookHistoricItems
                });
            }

            return resBookHistorics;
        }

        public Task<int> AddBookHistoricAsync(BookHistoric bookHistoric) => bookHistoricDAL.AddAsync(bookHistoric);
    }
}
