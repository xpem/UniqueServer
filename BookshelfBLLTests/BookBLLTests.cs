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
            Mock<DbSet<Book>> mockSetBook = new();

            IQueryable<Book> data = new List<BookshelfModels.Book>() {
                new Book() {
                    Title = "Teste de Título 2",
                    Authors = "Emanuel Teste",
                    Status = Convert.ToInt32(1),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    UserId = 1,
                    Id = 1
                } }.AsQueryable();

            mockSetBook.As<IQueryable<Book>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSetBook.As<IQueryable<Book>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSetBook.As<IQueryable<Book>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSetBook.As<IQueryable<Book>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            Mock<DbSet<BookHistoric>> mockSetBookHistoric = new();

            Mock<BookshelfDbContextDAL.BookshelfDbContext> mockContext = new();

            mockContext.Setup(m => m.Book).Returns(mockSetBook.Object);

            mockContext.Setup(m => m.BookHistoric).Returns(mockSetBookHistoric.Object);

            IBookHistoricBLL bookHistoricBLL = new BookHistoricBLL(mockContext.Object);

            IBookBLL bookBLL = new BookBLL(mockContext.Object, bookHistoricBLL);

            BookshelfModels.Request.ReqBook reqBook = new()
            {
                Title = "Teste de Título",
                Authors = "Emanuel Teste",
                Status = Convert.ToInt32(1)
            };

            BaseModels.BLLResponse response = bookBLL.CreateBook(reqBook, 1).Result;

            if (response.Content is not null)
                Assert.AreEqual(((ResBook)response.Content).Title, "Teste de Título");
            else Assert.Fail();
        }

        [TestMethod()]
        public void UpdateBookTest()
        {
            Mock<DbSet<Book>> mockSetBook = new();

            IQueryable<Book> data = new List<BookshelfModels.Book>() {
                new Book() {
                    Title = "Teste de Título",
                    Authors = "Emanuel Teste",
                    Status = Convert.ToInt32(1),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    UserId = 1,
                    Id = 1
                } }.AsQueryable();

            mockSetBook.As<IQueryable<Book>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSetBook.As<IQueryable<Book>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSetBook.As<IQueryable<Book>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSetBook.As<IQueryable<Book>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            Mock<DbSet<BookHistoric>> mockSetBookHistoric = new();

            Mock<DbSet<BookHistoricItem>> mockSetBookHistoricItem = new();

            Mock<BookshelfDbContextDAL.BookshelfDbContext> mockContext = new();

            mockContext.Setup(m => m.Book).Returns(mockSetBook.Object);

            mockContext.Setup(m => m.BookHistoric).Returns(mockSetBookHistoric.Object);

            mockContext.Setup(m => m.BookHistoricItem).Returns(mockSetBookHistoricItem.Object);

            IBookHistoricBLL bookHistoricBLL = new BookHistoricBLL(mockContext.Object);

            IBookBLL bookBLL = new BookBLL(mockContext.Object, bookHistoricBLL);

            BookshelfModels.Request.ReqBook reqBook = new()
            {
                Title = "Teste de Título alterado",
                Authors = "Emanuel Teste alterado",
                Status = Convert.ToInt32(1),
                Pages = 300
            };

            BaseModels.BLLResponse response = bookBLL.UpdateBook(reqBook, 1, 1).Result;

            if (response.Content is not null)
                Assert.AreEqual(((ResBook)response.Content).Title, "Teste de Título alterado");
            else Assert.Fail();
        }

        [TestMethod()]
        public void UpdateBook_Try_Update_Without_Changes_Test()
        {
            Mock<DbSet<Book>> mockSetBook = new();

            DateTime oldUpdatedAt = new DateTime(2023,01,01,01,01,01);

            IQueryable<Book> data = new List<BookshelfModels.Book>() {
                new Book() {
                    Title = "Teste de Título",
                    Authors = "Emanuel Teste",
                    Status = Convert.ToInt32(1),
                    Pages = 300,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = oldUpdatedAt,
                    UserId = 1,
                    Id = 1
                } }.AsQueryable();

            mockSetBook.As<IQueryable<Book>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSetBook.As<IQueryable<Book>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSetBook.As<IQueryable<Book>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSetBook.As<IQueryable<Book>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            Mock<DbSet<BookHistoric>> mockSetBookHistoric = new();

            Mock<DbSet<BookHistoricItem>> mockSetBookHistoricItem = new();

            Mock<BookshelfDbContextDAL.BookshelfDbContext> mockContext = new();

            mockContext.Setup(m => m.Book).Returns(mockSetBook.Object);

            mockContext.Setup(m => m.BookHistoric).Returns(mockSetBookHistoric.Object);

            mockContext.Setup(m => m.BookHistoricItem).Returns(mockSetBookHistoricItem.Object);

            IBookHistoricBLL bookHistoricBLL = new BookHistoricBLL(mockContext.Object);

            IBookBLL bookBLL = new BookBLL(mockContext.Object, bookHistoricBLL);

            BookshelfModels.Request.ReqBook reqBook = new()
            {
                Title = "Teste de Título",
                Authors = "Emanuel Teste",
                Status = Convert.ToInt32(1),
                Pages = 300
            };

            BaseModels.BLLResponse response = bookBLL.UpdateBook(reqBook, 1, 1).Result;

            if (response.Content is not null)
                Assert.AreEqual(((ResBook)response.Content).UpdatedAt, oldUpdatedAt);
            else Assert.Fail();
        }

        [TestMethod()]
        public void GetByUpdatedAtTest()
        {
            Mock<DbSet<Book>> mockSetBook = new();

            IQueryable<Book> data = new List<BookshelfModels.Book>() {
                new Book() {
                    Title = "Teste de Título",
                    Authors = "Emanuel Teste",
                    Status = Convert.ToInt32(1),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now.AddDays(-2),
                    UserId = 1,
                    Id = 1
                },
                new Book() {
                    Title = "Teste de Título 2",
                    Authors = "Emanuel Teste",
                    Status = Convert.ToInt32(1),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now.AddDays(-3),
                    UserId = 1,
                    Id = 2
                },
                     new Book() {
                    Title = "Teste de Título 3",
                    Authors = "Emanuel Teste",
                    Status = Convert.ToInt32(1),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now.AddHours(-2),
                    UserId = 1,
                    Id = 3
                },
                new Book() {
                    Title = "Teste de Título 4",
                    Authors = "Emanuel Teste",
                    Status = Convert.ToInt32(1),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now.AddHours(-1),
                    UserId = 1,
                    Id = 4
                },
                   new Book() {
                    Title = "Teste de Título 5",
                    Authors = "Emanuel Teste",
                    Status = Convert.ToInt32(1),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    UserId = 1,
                    Id = 5
                },
            }.AsQueryable();

            mockSetBook.As<IQueryable<Book>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSetBook.As<IQueryable<Book>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSetBook.As<IQueryable<Book>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSetBook.As<IQueryable<Book>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            Mock<BookshelfDbContextDAL.BookshelfDbContext> mockContext = new();
            mockContext.Setup(m => m.Book).Returns(mockSetBook.Object);

            IBookHistoricBLL bookHistoricBLL = new BookHistoricBLL(mockContext.Object);

            IBookBLL bookBLL = new BookBLL(mockContext.Object, bookHistoricBLL);

            BaseModels.BLLResponse response = bookBLL.GetByUpdatedAt(DateTime.Now.AddDays(-1), 1);

            List<ResBook>? resBook = new();

            if (response.Content != null)
                resBook = response.Content as List<ResBook>;

            if (response.Content != null && resBook?.Count == 3)
                Assert.IsTrue(true);
            else
                Assert.Fail();
        }
    }
}