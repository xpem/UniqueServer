using BookshelfModels;
using BookshelfModels.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BookshelfBLL.Tests
{
    [TestClass()]
    public class BookBLLTests
    {

        [TestMethod()]
        public void CreateBookTest()
        {
            var mockSetBook = new Mock<DbSet<BookshelfModels.Book>>();

            var data = new List<BookshelfModels.Book>() { new Book() { Title = "Teste de Título 2", Authors = "Emanuel Teste", Status = Convert.ToInt32(1), CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, UserId = 1 } }.AsQueryable();

            mockSetBook.As<IQueryable<Book>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSetBook.As<IQueryable<Book>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSetBook.As<IQueryable<Book>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSetBook.As<IQueryable<Book>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            var mockSetBookHistoric = new Mock<DbSet<BookshelfModels.BookHistoric>>();

            var mockContext = new Mock<BookshelfDbContextDAL.BookshelfDbContext>();

            mockContext.Setup(m => m.Book).Returns(mockSetBook.Object);

            mockContext.Setup(m => m.BookHistoric).Returns(mockSetBookHistoric.Object);

            IBookBLL bookBLL = new BookBLL(mockContext.Object);
            BookshelfModels.Request.ReqBook reqBook = new() { Title = "Teste de Título", Authors = "Emanuel Teste", Status = Convert.ToInt32(1) };
            var response = bookBLL.CreateBook(reqBook, 1).Result;

            if (response.Content is not null)
                Assert.AreEqual(((ResBook)response.Content).Title, "Teste de Título");
            else Assert.Fail();
        }
    }
}