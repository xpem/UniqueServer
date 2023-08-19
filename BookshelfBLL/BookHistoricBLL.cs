using BaseModels;
using BookshelfDbContextDAL;
using BookshelfModels;
using BookshelfModels.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookshelfBLL
{
    public class BookHistoricBLL : IBookHistoricBLL
    {
        private readonly BookshelfDbContext bookshelfDbContext;

        public BookHistoricBLL(BookshelfDbContext bookshelfDbContext)
        {
            this.bookshelfDbContext = bookshelfDbContext;
        }

        public async Task<BookHistoric> BuildAndCreateBookUpdateHistoric(Book oldBook, Book book)
        {
            var bookHistoricItemList = new List<BookshelfModels.BookHistoricItem>();

            BookHistoric bookHistoric = new()
            {
                BookId = book.Id,
                BookHistoricTypeId = 2,
                UserId = book.UserId,
                CreatedAt = DateTime.Now
            };

            await bookshelfDbContext.BookHistoric.AddAsync(bookHistoric);

            await bookshelfDbContext.SaveChangesAsync();

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

            await bookshelfDbContext.BookHistoricItem.AddRangeAsync(bookHistoricItemList);

            await bookshelfDbContext.SaveChangesAsync();

            bookHistoric.BookHistoricItems = bookHistoricItemList;

            return bookHistoric;
        }

        /// <summary>
        /// todo, otimizar isto
        /// </summary>
        public BLLResponse GetByCreatedAt(DateTime createdAt, int uid)
        {
            //se isso funcionar no banco real, adaptar o teste para se comportar igual
            var bookHistorics = bookshelfDbContext.BookHistoric.Where(x => x.UserId == uid && x.CreatedAt > createdAt)
                .Include(x => x.BookHistoricItems)
                .ThenInclude(bhi => bhi.BookHistoricItemField)
                .Include(x => x.BookHistoricType)
                .OrderByDescending(x => x.CreatedAt).ToList();

            List<ResBookHistoric> resBookHistorics = new();

            foreach( var _bookHistoric in bookHistorics)
            {
                List<ResBookHistoricItem> resBookHistoricItems = new();

                foreach (var _bookHistoricItem in _bookHistoric.BookHistoricItems)
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
                    TypeId = _bookHistoric.BookHistoricType.Id,
                    TypeName = _bookHistoric.BookHistoricType.Name,
                    CreatedAt = _bookHistoric.CreatedAt,
                    BookId = _bookHistoric.BookId,
                    BookHistoricItems = resBookHistoricItems
                });
            }


            return new BLLResponse() { Content = resBookHistorics };
        }
    }
}
