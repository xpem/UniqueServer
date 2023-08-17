using BookshelfDbContextDAL;
using BookshelfModels;
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

        public async Task BuildAndCreateBookUpdateHistoric(Book oldBook, Book book)
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

            await bookshelfDbContext.BookHistoricItem.AddRangeAsync(bookHistoricItemList);

        }
    }
}
