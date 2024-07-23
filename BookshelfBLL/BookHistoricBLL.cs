using BaseModels;
using BookshelfDAL;
using BookshelfModels;
using BookshelfModels.Response;

namespace BookshelfBLL
{

    public class BookHistoricBLL(IBookHistoricDAL bookHistoricDAL) : IBookHistoricBLL
    {
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

            await bookHistoricDAL.ExecuteAddRangeBookHistoricItemListAsync(bookHistoricItemList);

            bookHistoric.BookHistoricItems = bookHistoricItemList;

            return bookHistoric;
        }

        /// <summary>
        /// todo, otimizar isto
        /// </summary>
        public BLLResponse GetByBookIdOrCreatedAt(int? BookId, DateTime? createdAt, int uid)
        {
            List<BookHistoric> bookHistorics;

            if (BookId is not null)
                bookHistorics = bookHistoricDAL.ExecuteQueryByBookId(BookId.Value, uid);
            else
            {
                if (createdAt.HasValue)
                    bookHistorics = bookHistoricDAL.ExecuteQueryByCreatedAt(createdAt.Value, uid);
                else throw new NotImplementedException("Get sem parametro válido de busca");
            }
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

            return new BLLResponse(resBookHistorics);
        }

        public Task<int> AddBookHistoricAsync(BookHistoric bookHistoric) => bookHistoricDAL.ExecuteAddBookHistoricAsync(bookHistoric);
    }
}
