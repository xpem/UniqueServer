using BaseModels;
using InventoryDAL;
using InventoryDAL.Interfaces;
using InventoryDbContextDAL;
using InventoryModels;
using InventoryModels.Req;
using InventoryModels.Res;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace InventoryBLL.Tests
{
    [TestClass()]
    public class SubCategoryBLLTests
    {
        public static Mock<InventoryDbContext> BuildMockInventoryContext()
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
            mockContext.Setup(m => m.SaveChanges()).Returns(1);
            return mockContext;
        }

        [TestMethod()]
        public void GetByIdTest()
        {
            Mock<InventoryDbContext> mockContext = BuildMockInventoryContext();

            SubCategoryDAL bookHistoricDAL = new(mockContext.Object);

            SubCategoryBLL subCategoryBLL = new(bookHistoricDAL);

            BaseResponse bLLResponse = subCategoryBLL.GetById(1, 2);

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

            BaseResponse bLLResponse = subCategoryBLL.GetById(1, 3);

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

            BaseResponse bLLResponse = subCategoryBLL.GetByCategoryId(1, 1);

            if (bLLResponse?.Content is not null)
            {
                List<ResSubCategory>? resSubCategory = bLLResponse.Content as List<ResSubCategory>;

                Assert.IsTrue(resSubCategory?.Count == 3);
                return;
            }

            Assert.Fail();
        }

        [TestMethod()]
        public void CreateSubCategoryTest()
        {
            //Mock<InventoryDbContext> mockContext = BuildMockInventoryContext();

            Mock<ISubCategoryDAL> mockSubCategoryDAL = new();

            SubCategoryBLL subCategoryBLL = new(mockSubCategoryDAL.Object);

            ReqSubCategory reqSubCategory = new()
            {
                CategoryId = 2,
                Name = "Teste de título inserir",
                IconName = "Plate"
            };

            SubCategory? nullSubCategory = null;

            mockSubCategoryDAL.Setup(x => x.Create(It.IsAny<SubCategory>())).Returns(1);
            mockSubCategoryDAL.Setup(x => x.GetByCategoryIdAndName(1, reqSubCategory.CategoryId, reqSubCategory.Name)).Returns(nullSubCategory);

            BaseResponse bLLResponse = subCategoryBLL.CreateSubCategory(reqSubCategory, 1);

            if (bLLResponse?.Content is not null)
            {
                ResSubCategory? resSubCategory = bLLResponse?.Content as ResSubCategory;

                if (resSubCategory != null)
                    Assert.AreEqual(resSubCategory.Name, reqSubCategory.Name);
                else Assert.Fail();
            }
            else Assert.Fail();
        }

        [TestMethod()]
        public void UpdateSubCategoryTest()
        {
            Mock<InventoryDbContext> mockContext = BuildMockInventoryContext();

            Mock<SubCategoryDAL> mockSubCategoryDAL = new(mockContext.Object);

            SubCategoryBLL subCategoryBLL = new(mockSubCategoryDAL.Object);

            ReqSubCategory reqSubCategory = new()
            {
                CategoryId = 2,
                Name = "Teste de título alterado",
                IconName = "Plate"
            };

            //SubCategory? nullSubCategory = null;
            // mockSubCategoryDAL.Setup(x => x.UpdateSubCategory(It.IsAny<SubCategory>())).Returns(1);
            // mockSubCategoryDAL.Setup(x => x.GetByCategoryIdAndName(1, reqSubCategory.CategoryId, reqSubCategory.Name)).Returns(nullSubCategory);

            BaseResponse bLLResponse = subCategoryBLL.UpdateSubCategory(reqSubCategory, 2, 6);

            if (bLLResponse?.Content is not null)
            {
                ResSubCategory? resSubCategory = bLLResponse?.Content as ResSubCategory;

                if (resSubCategory != null)
                    Assert.AreEqual("Teste de título alterado", reqSubCategory.Name);
                else Assert.Fail();
            }
            else Assert.Fail();
        }

        [TestMethod()]
        public void Try_UpdateSubCategory_With_Same_Name_Test()
        {
            Mock<InventoryDbContext> mockContext = BuildMockInventoryContext();

            Mock<SubCategoryDAL> mockSubCategoryDAL = new(mockContext.Object);

            SubCategoryBLL subCategoryBLL = new(mockSubCategoryDAL.Object);

            ReqSubCategory reqSubCategory = new()
            {
                CategoryId = 2,
                Name = "Teste de título 4",
            };

            //SubCategory? nullSubCategory = null;
            // mockSubCategoryDAL.Setup(x => x.UpdateSubCategory(It.IsAny<SubCategory>())).Returns(1);
            // mockSubCategoryDAL.Setup(x => x.GetByCategoryIdAndName(1, reqSubCategory.CategoryId, reqSubCategory.Name)).Returns(nullSubCategory);

            BaseResponse bLLResponse = subCategoryBLL.UpdateSubCategory(reqSubCategory, 2, 6);

            if (bLLResponse?.Error is not null)
            {
                Error? errorMessage = bLLResponse?.Error;

                if (errorMessage?.Message != null)
                    Assert.AreEqual("A Sub Category with this Name has already been added to this Category", errorMessage.Message);
                else Assert.Fail();
            }
            else Assert.Fail();
        }

        [TestMethod()]
        public void Try_Update_A_SystemDefault_SubCategory__Test()
        {
            Mock<InventoryDbContext> mockContext = BuildMockInventoryContext();

            Mock<SubCategoryDAL> mockSubCategoryDAL = new(mockContext.Object);

            SubCategoryBLL subCategoryBLL = new(mockSubCategoryDAL.Object);

            ReqSubCategory reqSubCategory = new()
            {
                CategoryId = 2,
                Name = "Teste de título 4",
            };

            //SubCategory? nullSubCategory = null;
            // mockSubCategoryDAL.Setup(x => x.UpdateSubCategory(It.IsAny<SubCategory>())).Returns(1);
            // mockSubCategoryDAL.Setup(x => x.GetByCategoryIdAndName(1, reqSubCategory.CategoryId, reqSubCategory.Name)).Returns(nullSubCategory);

            BaseResponse bLLResponse = subCategoryBLL.UpdateSubCategory(reqSubCategory, 2, 5);

            if (bLLResponse?.Error is not null)
            {
                Error? errorMessage = bLLResponse?.Error;

                if (errorMessage?.Message != null)
                    Assert.AreEqual("It's not possible edit a system default Sub Category", errorMessage.Message);
                else Assert.Fail();
            }
            else Assert.Fail();
        }

        [TestMethod()]
        public void DeleteSubCategoryTest()
        {
            Mock<InventoryDbContext> mockContext = BuildMockInventoryContext();

            Mock<SubCategoryDAL> mockSubCategoryDAL = new(mockContext.Object);

            SubCategoryBLL subCategoryBLL = new(mockSubCategoryDAL.Object);

            //SubCategory? nullSubCategory = null;
            // mockSubCategoryDAL.Setup(x => x.UpdateSubCategory(It.IsAny<SubCategory>())).Returns(1);
            // mockSubCategoryDAL.Setup(x => x.GetByCategoryIdAndName(1, reqSubCategory.CategoryId, reqSubCategory.Name)).Returns(nullSubCategory);

            BaseResponse bLLResponse = subCategoryBLL.DeleteSubCategory(2, 6);

            if (bLLResponse?.Error is null && bLLResponse?.Content is null)
                Assert.IsTrue(true);
            else Assert.Fail();
        }

        [TestMethod()]
        public void Try_Delete_SystemDefault_SubCategoryTest()
        {
            Mock<InventoryDbContext> mockContext = BuildMockInventoryContext();

            Mock<SubCategoryDAL> mockSubCategoryDAL = new(mockContext.Object);

            SubCategoryBLL subCategoryBLL = new(mockSubCategoryDAL.Object);

            //SubCategory? nullSubCategory = null;
            // mockSubCategoryDAL.Setup(x => x.UpdateSubCategory(It.IsAny<SubCategory>())).Returns(1);
            // mockSubCategoryDAL.Setup(x => x.GetByCategoryIdAndName(1, reqSubCategory.CategoryId, reqSubCategory.Name)).Returns(nullSubCategory);

            BaseResponse bLLResponse = subCategoryBLL.DeleteSubCategory(2, 5);

            if (bLLResponse?.Error is not null && bLLResponse?.Content is null)
            {
                Error? errorMessage = bLLResponse?.Error;

                if (errorMessage?.Message != null)
                {
                    Assert.AreEqual("It's not possible delete a system default Sub Category", errorMessage.Message);
                    return;
                }
            }
            else Assert.Fail();
        }

    }
}