using BookshelfDAL;
using BookshelfDbContextDAL;
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
            IQueryable<Book> listBook = new List<BookshelfModels.Book>() {
                new() {
                    Title = "Teste de Título 2",
                    Authors = "Emanuel Teste",
                    Status = Convert.ToInt32(1),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    UserId = 1,
                    Id = 1
                } }.AsQueryable();

            BookshelfModels.Request.ReqBook reqBook = new()
            {
                Title = "Teste de Título",
                Authors = "Emanuel Teste",
                Status = Convert.ToInt32(1)
            };

            Book? bookByTitleAsync = null;

            Mock<BookshelfDbContextDAL.BookshelfDbContext> mockContext = new();

            Mock<IBookHistoricDAL> mockBookHistoricDAL = new();
            Mock<IBookRepo> mockBookDAL = new();

            mockBookDAL.Setup(x => x.ExecuteCreateBookAsync(It.IsAny<Book>())).ReturnsAsync(1);
            mockBookDAL.Setup(x => x.GetBookByTitleAsync(reqBook.Title, 1)).ReturnsAsync(bookByTitleAsync);

            mockBookHistoricDAL.Setup(x => x.ExecuteAddBookHistoricAsync(It.IsAny<BookHistoric>())).ReturnsAsync(1);

            IBookHistoricBLL bookHistoricBLL = new BookHistoricBLL(mockBookHistoricDAL.Object);

            BookService bookBLL = new(bookHistoricBLL, mockBookDAL.Object);

            BaseModels.BaseResponse response = bookBLL.CreateAsync(reqBook, 1).Result;

            if (response.Content is not null)
                Assert.AreEqual(((ResBook)response.Content).Title, "Teste de Título");
            else Assert.Fail();
        }

        [TestMethod()]
        public void UpdateBookTest()
        {
            Mock<DbSet<Book>> mockSetBook = new();

            Book OriBook = new()
            {
                Title = "Teste de Título",
                Authors = "Emanuel Teste",
                Status = Convert.ToInt32(1),
                Pages = 300,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                UserId = 1,
                Id = 1
            };

            Mock<IBookRepo> mockBookDAL = new();
            Mock<IBookHistoricDAL> mockBookHistoricDAL = new();

            mockBookHistoricDAL.Setup(x => x.ExecuteAddBookHistoricAsync(It.IsAny<BookHistoric>())).ReturnsAsync(1);

            mockBookHistoricDAL.Setup(x => x.ExecuteAddRangeBookHistoricItemListAsync(It.IsAny<List<BookHistoricItem>>())).ReturnsAsync(1);

            IBookHistoricBLL bookHistoricBLL = new BookHistoricBLL(mockBookHistoricDAL.Object);

            Book? bookByTitleWithNotEqualId = null;

            BookshelfModels.Request.ReqBook reqBook = new()
            {
                Title = "Teste de Título alterado",
                Authors = "Emanuel Teste alterado",
                Status = Convert.ToInt32(1),
                Pages = 300
            };

            mockBookDAL.Setup(x => x.GetBookByTitleWithNotEqualIdAsync(reqBook.Title, 1, 1)).ReturnsAsync(bookByTitleWithNotEqualId);

            mockBookDAL.Setup(x => x.GetBookByIdAsync(1, 1)).ReturnsAsync(OriBook);

            mockBookDAL.Setup(x => x.ExecuteUpdateBookAsync(It.IsAny<Book>())).ReturnsAsync(1);

            BookService bookBLL = new(bookHistoricBLL, mockBookDAL.Object);

            BaseModels.BaseResponse response = bookBLL.UpdateAsync(reqBook, 1, 1).Result;

            if (response.Content is not null)
                Assert.AreEqual(((ResBook)response.Content).Title, "Teste de Título alterado");
            else Assert.Fail();
        }

        [TestMethod()]
        public void UpdateBookStatusTest()
        {
            Mock<DbSet<Book>> mockSetBook = new();

            Book OriBook = new()
            {
                Title = "Teste de Título",
                Authors = "Emanuel Teste",
                Status = Convert.ToInt32(1),
                Pages = 300,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                UserId = 1,
                Id = 1
            };

            Mock<IBookRepo> mockBookDAL = new();
            Mock<IBookHistoricDAL> mockBookHistoricDAL = new();

            mockBookHistoricDAL.Setup(x => x.ExecuteAddBookHistoricAsync(It.IsAny<BookHistoric>())).ReturnsAsync(1);

            mockBookHistoricDAL.Setup(x => x.ExecuteAddRangeBookHistoricItemListAsync(It.IsAny<List<BookHistoricItem>>())).ReturnsAsync(1);

            IBookHistoricBLL bookHistoricBLL = new BookHistoricBLL(mockBookHistoricDAL.Object);

            Book? bookByTitleWithNotEqualId = null;

            BookshelfModels.Request.ReqBook reqBook = new()
            {
                Title = OriBook.Title,
                Authors = OriBook.Authors,

                Status = Convert.ToInt32(2),
                Score = Convert.ToInt32(3),
                Comment = "Teste de comentário",

                Pages = OriBook.Pages
            };

            mockBookDAL.Setup(x => x.GetBookByTitleWithNotEqualIdAsync(reqBook.Title, 1, 1)).ReturnsAsync(bookByTitleWithNotEqualId);

            mockBookDAL.Setup(x => x.GetBookByIdAsync(1, 1)).ReturnsAsync(OriBook);

            mockBookDAL.Setup(x => x.ExecuteUpdateBookAsync(It.IsAny<Book>())).ReturnsAsync(1);

            BookService bookBLL = new(bookHistoricBLL, mockBookDAL.Object);

            BaseModels.BaseResponse response = bookBLL.UpdateAsync(reqBook, 1, 1).Result;

            if (response.Content is not null)
                Assert.AreEqual(((ResBook)response.Content).Status, 2);
            else Assert.Fail();
        }

        [TestMethod()]
        public void UpdateBook_Try_Update_Without_Changes_Test()
        {
            Mock<DbSet<Book>> mockSetBook = new();

            DateTime oldUpdatedAt = new(2023, 01, 01, 01, 01, 01);

            Book OriBook = new()
            {
                Title = "Teste de Título",
                Authors = "Emanuel Teste",
                Status = Convert.ToInt32(1),
                Pages = 300,
                CreatedAt = DateTime.Now,
                UpdatedAt = oldUpdatedAt,
                UserId = 1,
                Id = 1
            };

            Mock<DbSet<BookHistoric>> mockSetBookHistoric = new();

            Mock<DbSet<BookHistoricItem>> mockSetBookHistoricItem = new();

            Mock<BookshelfDbContextDAL.BookshelfDbContext> mockContext = new();

            mockContext.Setup(m => m.Book).Returns(mockSetBook.Object);

            mockContext.Setup(m => m.BookHistoric).Returns(mockSetBookHistoric.Object);

            mockContext.Setup(m => m.BookHistoricItem).Returns(mockSetBookHistoricItem.Object);

            Mock<IBookRepo> mockBookDAL = new();
            Mock<IBookHistoricDAL> mockBookHistoricDAL = new();

            mockBookHistoricDAL.Setup(x => x.ExecuteAddBookHistoricAsync(It.IsAny<BookHistoric>())).ReturnsAsync(1);

            mockBookHistoricDAL.Setup(x => x.ExecuteAddRangeBookHistoricItemListAsync(It.IsAny<List<BookHistoricItem>>())).ReturnsAsync(1);

            IBookHistoricBLL bookHistoricBLL = new BookHistoricBLL(mockBookHistoricDAL.Object);

            BookshelfModels.Request.ReqBook reqBook = new()
            {
                Title = "Teste de Título",
                Authors = "Emanuel Teste",
                Status = Convert.ToInt32(1),
                Pages = 300
            };

            Book? bookByTitleWithNotEqualId = null;

            mockBookDAL.Setup(x => x.GetBookByTitleWithNotEqualIdAsync(reqBook.Title, 1, 1)).ReturnsAsync(bookByTitleWithNotEqualId);

            mockBookDAL.Setup(x => x.GetBookByIdAsync(1, 1)).ReturnsAsync(OriBook);

            mockBookDAL.Setup(x => x.ExecuteUpdateBookAsync(It.IsAny<Book>())).ReturnsAsync(1);

            IBookService bookBLL = new BookService(bookHistoricBLL, mockBookDAL.Object);

            BaseModels.BaseResponse response = bookBLL.UpdateAsync(reqBook, 1, 1).Result;

            if (response.Content is not null)
                Assert.AreEqual(((ResBook)response.Content).UpdatedAt, oldUpdatedAt);
            else Assert.Fail();
        }

        [TestMethod()]
        public void GetByUpdatedAtTest()
        {
            Mock<DbSet<Book>> mockSetBook = new();

            List<Book> list = [
                new Book()
                {
                    Title = "Teste de Título",
                    Authors = "Emanuel Teste",
                    Status = Convert.ToInt32(1),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now.AddDays(-2),
                    UserId = 1,
                    Id = 1
                },
                new Book()
                {
                    Title = "Teste de Título 2",
                    Authors = "Emanuel Teste",
                    Status = Convert.ToInt32(1),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now.AddDays(-3),
                    UserId = 1,
                    Id = 2
                }];

            List<Book> listResult = [
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
                } ];

            list.AddRange(listResult);

            IQueryable<Book> data = list.AsQueryable();

            IQueryable<Book> dataResult = listResult.AsQueryable();

            mockSetBook.As<IQueryable<Book>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSetBook.As<IQueryable<Book>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSetBook.As<IQueryable<Book>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSetBook.As<IQueryable<Book>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            Mock<BookshelfDbContextDAL.BookshelfDbContext> mockContext = new();
            mockContext.Setup(m => m.Book).Returns(mockSetBook.Object);

            Mock<IBookHistoricDAL> mockBookHistoricDAL = new();
            Mock<IBookRepo> mockBookDAL = new();

            IBookHistoricBLL bookHistoricBLL = new BookHistoricBLL(mockBookHistoricDAL.Object);

            DateTime updatedAt = DateTime.Now.AddDays(-1);

            mockBookDAL.Setup(x => x.GetBooksAfterUpdatedAt(updatedAt, 1)).Returns(dataResult);

            IBookService bookBLL = new BookService(bookHistoricBLL, mockBookDAL.Object);

            BaseModels.BaseResponse response = bookBLL.GetByUpdatedAt(updatedAt, 1);

            List<ResBook>? resBook = [];

            if (response.Content != null)
                resBook = response.Content as List<ResBook>;

            if (response.Content != null && resBook?.Count == 3)
                Assert.IsTrue(true);
            else
                Assert.Fail();
        }

        [TestMethod()]
        public void InactivateBookTest()
        {
            Mock<DbSet<Book>> mockSetBook = new();

            DateTime oldUpdatedAt = new(2023, 01, 01, 01, 01, 01);

            Mock<DbSet<BookHistoric>> mockSetBookHistoric = new();

            Mock<DbSet<BookHistoricItem>> mockSetBookHistoricItem = new();

            Mock<BookshelfDbContextDAL.BookshelfDbContext> mockContext = new();

            Mock<IBookHistoricDAL> mockBookHistoricDAL = new();

            mockBookHistoricDAL.Setup(x => x.ExecuteAddBookHistoricAsync(It.IsAny<BookHistoric>())).ReturnsAsync(1);

            IBookHistoricBLL bookHistoricBLL = new BookHistoricBLL(mockBookHistoricDAL.Object);

            Mock<IBookRepo> mockBookDAL = new();

            Book OriBook = new()
            {
                Title = "Teste de Título",
                Authors = "Emanuel Teste",
                Status = Convert.ToInt32(1),
                Pages = 300,
                CreatedAt = DateTime.Now,
                UpdatedAt = oldUpdatedAt,
                UserId = 1,
                Id = 1
            };

            mockBookDAL.Setup(x => x.GetBookByIdAsync(1, 1)).ReturnsAsync(OriBook);

            mockBookDAL.Setup(x => x.ExecuteInactivateBookAsync(1, 1)).ReturnsAsync(1);

            IBookService bookBLL = new BookService(bookHistoricBLL, mockBookDAL.Object);

            BaseModels.BaseResponse response = bookBLL.InactivateAsync(1, 1).Result;

            if (response.Content is not null)
                Assert.IsTrue(Convert.ToBoolean(response.Content));
            else Assert.Fail();
        }
    }
}