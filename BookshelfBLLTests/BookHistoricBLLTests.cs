using BookshelfDAL;
using BookshelfModels;
using BookshelfModels.Response;
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
            Mock<IBookHistoricRepo> mockBookHistoricDAL = new();

            mockBookHistoricDAL.Setup(x => x.ExecuteAddBookHistoricAsync(It.IsAny<BookHistoric>())).ReturnsAsync(1);

            mockBookHistoricDAL.Setup(x => x.ExecuteAddRangeBookHistoricItemListAsync(It.IsAny<List<BookHistoricItem>>())).ReturnsAsync(1);

            BookHistoricService bookHistoricBLL = new(mockBookHistoricDAL.Object);

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

            BookHistoric result = bookHistoricBLL.BuildAndCreateBookUpdateHistoricAsync(oldBook, book).Result;

            if (result != null && result.BookHistoricItems is not null)
                Assert.IsTrue(result.BookHistoricItems.Count == 3);
            else Assert.Fail();
        }

        [TestMethod()]
        public void GetByCreatedAtTest()
        {
            Mock<BookshelfDbContextDAL.BookshelfDbContext> mockContext = new();

            List<BookshelfModels.BookHistoric> dataBH = [
                new BookHistoric()
                {
                    Id = 6,
                    CreatedAt = DateTime.Now.AddDays(-1),
                    BookHistoricTypeId = 2,
                    BookId = 209,
                    UserId = 1,
                    BookHistoricItems = [
                        new()
                        {
                            BookHistoricId = 6,
                            BookHistoricItemFieldId = 6,
                            BookHistoricItemField = new BookHistoricItemField()
                            {
                                Id = 6,
                                Name = "Páginas",
                            },
                            CreatedAt = DateTime.Now.AddDays(-1),
                            UpdatedFrom = "3",
                            UpdatedTo = "4",
                            Id = 11,
                        },
                        new()
                        {
                            BookHistoricId = 6,
                            BookHistoricItemFieldId = 6,
                            BookHistoricItemField = new BookHistoricItemField()
                            {
                                Id = 4,
                                Name = "Autores",
                            },
                            CreatedAt = DateTime.Now.AddDays(-1),
                            UpdatedFrom = "teste 3 alteracao 2",
                            UpdatedTo = "teste 3 alteracao autor 3",
                        }
                    ]
                },
            ];

            Mock<IBookHistoricRepo> mockBookHistoricDAL = new();

            DateTime dataBusca = DateTime.Now.AddDays(-1).AddHours(-2);

            mockBookHistoricDAL.Setup(x => x.ExecuteQueryByCreatedAt(dataBusca, 1)).Returns(dataBH);

            BookHistoricService bookHistoricBLL = new(mockBookHistoricDAL.Object);

            BaseModels.BaseResponse response = bookHistoricBLL.GetByBookIdOrCreatedAt(null, dataBusca, 1);

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

        [TestMethod()]
        public void GetByBookIdOrCreatedAtTest()
        {
            Mock<IBookHistoricRepo> mockBookHistoricDAL = new();

            List<BookshelfModels.BookHistoric> dataBH = [
                new BookHistoric()
                {
                    Id = 6,
                    CreatedAt = DateTime.Now.AddDays(-1),
                    BookHistoricTypeId = 2,
                    BookId = 210,
                    UserId = 1,
                    BookHistoricItems = [
                        new()
                        {
                            BookHistoricId = 6,
                            BookHistoricItemFieldId = 6,
                            BookHistoricItemField = new BookHistoricItemField()
                            {
                                Id = 6,
                                Name = "Páginas",
                            },
                            CreatedAt = DateTime.Now.AddDays(-1),
                            UpdatedFrom = "3",
                            UpdatedTo = "4",
                            Id = 11,
                        },
                        new()
                        {
                            BookHistoricId = 6,
                            BookHistoricItemFieldId = 6,
                            BookHistoricItemField = new BookHistoricItemField()
                            {
                                Id = 4,
                                Name = "Autores",
                            },
                            CreatedAt = DateTime.Now.AddDays(-1),
                            UpdatedFrom = "teste 3 alteracao 2",
                            UpdatedTo = "teste 3 alteracao autor 3",
                        }
                    ]
                },
            ];

            mockBookHistoricDAL.Setup(x => x.ExecuteQueryByBookId(210, 1)).Returns(dataBH);

            IBookHistoricService bookHistoricBLL = new BookHistoricService(mockBookHistoricDAL.Object);

            BaseModels.BaseResponse response = bookHistoricBLL.GetByBookIdOrCreatedAt(210, null, 1);

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