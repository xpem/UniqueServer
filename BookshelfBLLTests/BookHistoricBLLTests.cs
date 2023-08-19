using BookshelfBLL;
using BookshelfModels;
using BookshelfModels.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BookshelfBLL.Tests
{
    [TestClass()]
    public class BookHistoricBLLTests
    {
        [TestMethod()]
        public void BuildAndSaveBookUpdateHistoricTest()
        {
            var mockContext = new Mock<BookshelfDbContextDAL.BookshelfDbContext>();

            var mockSetBookHistoric = new Mock<DbSet<BookshelfModels.BookHistoric>>();

            mockContext.Setup(m => m.BookHistoric).Returns(mockSetBookHistoric.Object);

            var mockSetBookHistoricItem = new Mock<DbSet<BookshelfModels.BookHistoricItem>>();

            mockContext.Setup(m => m.BookHistoricItem).Returns(mockSetBookHistoricItem.Object);

            mockContext.Setup(c => c.SaveChangesAsync(default)).Returns(Task.FromResult(1)).Verifiable();

            IBookHistoricBLL bookHistoricBLL = new BookHistoricBLL(mockContext.Object);

            Book oldBook = new()
            {
                Title = "Teste de Título",
                Subtitle = "Teste de SubTítulo",
                Authors = "Emanuel Teste",
                Status = 2,
                UpdatedAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UserId = 1
            };

            Book book = new()
            {
                Title = "Teste de Título",
                Subtitle = "Teste de SubTítulo Alterado",
                Authors = "Emanuel Teste",
                Status = 3,
                Score = 2,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                UserId = 1
            };

            var result = bookHistoricBLL.BuildAndCreateBookUpdateHistoric(oldBook, book).Result;

            if (result != null && result.BookHistoricItems is not null)
                Assert.IsTrue(result.BookHistoricItems.Count == 3);
            else Assert.Fail();
        }

        [TestMethod()]
        public void GetByCreatedAtTest()
        {

            var mockSetBH = new Mock<DbSet<BookshelfModels.BookHistoric>>();
            var mockSetBHI = new Mock<DbSet<BookshelfModels.BookHistoricItem>>();
            var mockSetBHT = new Mock<DbSet<BookshelfModels.BookHistoricType>>();
            var mockSetBHIF = new Mock<DbSet<BookshelfModels.BookHistoricItemField>>();

            var dataBH = new List<BookshelfModels.BookHistoric>() {
             new BookHistoric()
             {
                 Id = 7,
                 CreatedAt= DateTime.Now.AddDays(-2),
                 BookHistoricTypeId = 2,
                 BookHistoricType =
                 new BookHistoricType()
                 {
                     Id = 2,
                     Name="Update"
                 },
                 BookId = 209,
                 UserId= 1,
                 BookHistoricItems =  new List<BookshelfModels.BookHistoricItem>() {
                     new BookHistoricItem()
                     {
                         BookHistoricId = 7,
                         BookHistoricItemFieldId = 5,
                         BookHistoricItemField =       new BookHistoricItemField()
            {
                Id = 5,
                Name = "Volume",
            },
                         CreatedAt = DateTime.Now.AddDays(-2),
                         UpdatedFrom = "4",
                         UpdatedTo = "5",
                         Id = 11,
                     },
                     new BookHistoricItem()
                     {
                         BookHistoricId = 7,
                         BookHistoricItemFieldId = 6,
                         CreatedAt = DateTime.Now.AddDays(-2),
                         UpdatedFrom = "265",
                         UpdatedTo = "275",
                     },
                     new BookHistoricItem()
                     {
                         BookHistoricId = 7,
                         BookHistoricItemFieldId = 7,
                         BookHistoricItemField =   new BookHistoricItemField()
            {
                Id = 7,
                Name = "Ano",
            },
                         CreatedAt = DateTime.Now.AddDays(-2),
                         UpdatedFrom = "2038",
                         UpdatedTo = "2048",
                     }
                 },
             },
                new BookHistoric()
                {
                    Id = 6,
                    CreatedAt= DateTime.Now.AddDays(-1),
                    BookHistoricTypeId = 2,
                    BookId = 209,
                    UserId= 1,
                    BookHistoricItems =  new List<BookshelfModels.BookHistoricItem>() {
                        new BookHistoricItem()
                        {
                            BookHistoricId = 6,
                            BookHistoricItemFieldId = 6,
                            BookHistoricItemField =  new BookHistoricItemField()
            {
                Id = 6,
                Name = "Páginas",
            },
                            CreatedAt = DateTime.Now.AddDays(-1),
                            UpdatedFrom = "3",
                            UpdatedTo = "4",
                            Id = 11,
                        },
                        new BookHistoricItem()
                        {
                            BookHistoricId = 6,
                            BookHistoricItemFieldId = 6,
                            BookHistoricItemField =  new BookHistoricItemField()
            {
                Id = 4,
                Name = "Autores",
            },
                            CreatedAt = DateTime.Now.AddDays(-1),
                            UpdatedFrom = "teste 3 alteracao 2",
                            UpdatedTo = "teste 3 alteracao autor 3",
                        }
                    }
                },
            }.AsQueryable();

            mockSetBH.As<IQueryable<BookHistoric>>().Setup(m => m.Provider).Returns(dataBH.Provider);
            mockSetBH.As<IQueryable<BookHistoric>>().Setup(m => m.Expression).Returns(dataBH.Expression);
            mockSetBH.As<IQueryable<BookHistoric>>().Setup(m => m.ElementType).Returns(dataBH.ElementType);
            mockSetBH.As<IQueryable<BookHistoric>>().Setup(m => m.GetEnumerator()).Returns(() => dataBH.GetEnumerator());

            var mockContext = new Mock<BookshelfDbContextDAL.BookshelfDbContext>();
            mockContext.Setup(m => m.BookHistoric).Returns(mockSetBH.Object);
            mockContext.Setup(m => m.BookHistoricItem).Returns(mockSetBHI.Object);
            mockContext.Setup(m => m.BookHistoricType).Returns(mockSetBHT.Object);
            mockContext.Setup(m => m.BookHistoricItemField).Returns(mockSetBHIF.Object);

            IBookHistoricBLL bookHistoricBLL = new BookHistoricBLL(mockContext.Object);

            var response = bookHistoricBLL.GetByCreatedAt(DateTime.Now.AddDays(-1).AddHours(-2), 1);

            if (response != null && response.Content != null)
            {
                List<ResBookHistoric>? responseList = (response.Content as List<ResBookHistoric>);

                if (responseList is not null && responseList.Count == 1 && responseList.First().BookHistoricItems?.Count == 2)
                    Assert.IsTrue(true);
                else Assert.Fail();
            }
            else
                Assert.Fail();
        }
    }
}