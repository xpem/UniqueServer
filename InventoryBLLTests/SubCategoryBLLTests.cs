using Microsoft.VisualStudio.TestTools.UnitTesting;
using InventoryBLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InventoryModels;
using Moq;
using InventoryDbContextDAL;
using InventoryDAL;
using BaseModels;
using InventoryModels.Res;

namespace InventoryBLL.Tests
{
    [TestClass()]
    public class SubCategoryBLLTests
    {
        private Mock<InventoryDbContext> BuildMockInventoryContext()
        {
            Mock<DbSet<SubCategory>> mockSetSubCategory = new();

            List<SubCategory> SubCategories = [
                new SubCategory()
                {
                    Id = 1,
                    CategoryId = 1,
                    CreatedAt = DateTime.Now,
                    Name = "Teste de título 1",
                    SystemDefault = true,
                    IconName = "Dog",
                },
                new SubCategory()
                {
                    Id = 2,
                    CategoryId = 1,
                    CreatedAt = DateTime.Now,
                    Name = "Teste de título 2",
                    SystemDefault = true,
                    IconName = "Cat",
                },
                new SubCategory()
                {
                    Id = 3,
                    CategoryId = 1,
                    CreatedAt = DateTime.Now,
                    Name = "Teste de título 2.1",
                    SystemDefault = false,
                    IconName = "Bird",
                    UserId = 1,
                },
                new SubCategory()
                {
                    Id = 4,
                    CategoryId = 2,
                    CreatedAt = DateTime.Now,
                    Name = "Teste de título 3",
                    SystemDefault = true,
                    IconName = "Chair",
                },
                new SubCategory()
                {
                    Id = 5,
                    CategoryId = 2,
                    CreatedAt = DateTime.Now,
                    Name = "Teste de título 4",
                    SystemDefault = true,
                    IconName = "Table",
                },
                new SubCategory()
                {
                    Id = 6,
                    CategoryId = 2,
                    CreatedAt = DateTime.Now,
                    Name = "Teste de título 5",
                    SystemDefault = false,
                    IconName = "Plate",
                    UserId = 2,
                },
            ];

            IQueryable<SubCategory> data = SubCategories.AsQueryable();

            mockSetSubCategory.As<IQueryable<SubCategory>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSetSubCategory.As<IQueryable<SubCategory>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSetSubCategory.As<IQueryable<SubCategory>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSetSubCategory.As<IQueryable<SubCategory>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            Mock<InventoryDbContext> mockContext = new();
            mockContext.Setup(m => m.SubCategory).Returns(mockSetSubCategory.Object);
            return mockContext;
        }

        [TestMethod()]
        public void GetByIdTest()
        {
            Mock<InventoryDbContext> mockContext = BuildMockInventoryContext();

            SubCategoryDAL bookHistoricDAL = new(mockContext.Object);

            SubCategoryBLL subCategoryBLL = new(bookHistoricDAL);

            BLLResponse bLLResponse = subCategoryBLL.GetById(1, 2);

            if (bLLResponse?.Content is not null)
            {
                ResSubCategory? resSubCategory = bLLResponse.Content as ResSubCategory;

                Assert.IsTrue(resSubCategory?.Id == 2);
            }
        }

        [TestMethod()]
        public void GetById_SytemDefault_Test()
        {
            Mock<InventoryDbContext> mockContext = BuildMockInventoryContext();

            SubCategoryDAL bookHistoricDAL = new(mockContext.Object);

            SubCategoryBLL subCategoryBLL = new(bookHistoricDAL);

            BLLResponse bLLResponse = subCategoryBLL.GetById(1, 3);

            if (bLLResponse?.Content is not null)
            {
                ResSubCategory? resSubCategory = bLLResponse.Content as ResSubCategory;

                Assert.IsTrue(resSubCategory?.Id == 3);
            }
        }

        [TestMethod()]
        public void GetByCategoryIdTest()
        {
            Mock<InventoryDbContext> mockContext = BuildMockInventoryContext();

            SubCategoryDAL bookHistoricDAL = new(mockContext.Object);

            SubCategoryBLL subCategoryBLL = new(bookHistoricDAL);

            BLLResponse bLLResponse = subCategoryBLL.GetByCategoryId(1, 1);

            if (bLLResponse?.Content is not null)
            {
                List<ResSubCategory>? resSubCategory = bLLResponse.Content as List<ResSubCategory>;

                Assert.IsTrue(resSubCategory?.Count == 3);
            }
        }
    }
}