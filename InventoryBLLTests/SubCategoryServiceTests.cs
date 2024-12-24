using BaseModels;
using InventoryModels.DTOs;
using InventoryModels.Req;
using InventoryModels.Res;
using InventoryRepos.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace InventoryBLL.Tests
{

    [TestClass()]
    public class SubCategoryServiceTests
    {
        private readonly IQueryable<SubCategory> SubCategories = new List<SubCategory>() {
                 new SubCategory()
                {
                    Id = 1,
                    CategoryId = 1,
                    CreatedAt = DateTime.Now,
                    Name = "Teste de título 1",
                    SystemDefault = true,
                    IconName = "Dog"
                },
                new SubCategory()
                {
                    Id = 2,
                    CategoryId = 1,
                    CreatedAt = DateTime.Now,
                    Name = "Teste de título 2",
                    SystemDefault = true,
                    IconName = "Cat"
                },
                new SubCategory()
                {
                    Id = 3,
                    CategoryId = 1,
                    CreatedAt = DateTime.Now,
                    Name = "Teste de título 2.1",
                    SystemDefault = false,
                    IconName = "Bird",
                    UserId = 1
                },
                new SubCategory()
                {
                    Id = 4,
                    CategoryId = 2,
                    CreatedAt = DateTime.Now,
                    Name = "Teste de título 3",
                    SystemDefault = true,
                    IconName = "Chair"
                },
                new SubCategory()
                {
                    Id = 5,
                    CategoryId = 2,
                    CreatedAt = DateTime.Now,
                    Name = "Teste de título 4",
                    SystemDefault = true,
                    IconName = "Table"
                },
                new SubCategory()
                {
                    Id = 6,
                    CategoryId = 2,
                    CreatedAt = DateTime.Now,
                    Name = "Teste de título 5",
                    SystemDefault = false,
                    IconName = "Plate",
                    UserId = 2
                },
        }.AsQueryable();

        [TestMethod()]
        public void GetByIdTest()
        {
            SubCategory subCategory =
                new SubCategory()
                {
                    Id = 2,
                    CategoryId = 1,
                    CreatedAt = DateTime.Now,
                    Name = "Teste de título 2",
                    SystemDefault = true,
                    IconName = "Cat",
                };

            Mock<ISubCategoryRepo> subCategoryDAL = new Mock<ISubCategoryRepo>();
            subCategoryDAL.Setup(x => x.GetById(1, 2)).Returns(subCategory);

            //Mock<InventoryDbContext> mockContext = BuildMockInventoryContext();

            SubCategoryService subCategoryBLL = new(subCategoryDAL.Object);

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
            SubCategory subCategory =
              new SubCategory()
              {
                  Id = 3,
                  CategoryId = 1,
                  CreatedAt = DateTime.Now,
                  Name = "Teste de título 2",
                  SystemDefault = true,
                  IconName = "Cat",
              };
            Mock<ISubCategoryRepo> subCategoryDAL = new Mock<ISubCategoryRepo>();
            subCategoryDAL.Setup(x => x.GetById(1, 2)).Returns(subCategory);

            SubCategoryService subCategoryBLL = new(subCategoryDAL.Object);

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
                    UserId = 1
                }];

            Mock<ISubCategoryRepo> subCategoryDAL = new Mock<ISubCategoryRepo>();
            subCategoryDAL.Setup(x => x.GetByCategoryId(1, 1)).Returns(SubCategories);

            SubCategoryService subCategoryBLL = new(subCategoryDAL.Object);

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
        public async Task CreateSubCategoryTest()
        {
            //Mock<InventoryDbContext> mockContext = BuildMockInventoryContext();

            Mock<ISubCategoryRepo> mockSubCategoryDAL = new();

            SubCategoryService subCategoryBLL = new(mockSubCategoryDAL.Object);

            ReqSubCategory reqSubCategory = new()
            {
                CategoryId = 2,
                Name = "Teste de título inserir",
                IconName = "Plate"
            };

            SubCategory? nullSubCategory = null;

            mockSubCategoryDAL.Setup(x => x.CreateAsync(It.IsAny<SubCategory>())).ReturnsAsync(1);
            mockSubCategoryDAL.Setup(x => x.GetByCategoryIdAndName(1, reqSubCategory.CategoryId, reqSubCategory.Name)).Returns(nullSubCategory);

            BaseResponse bLLResponse = await subCategoryBLL.CreateSubCategoryAsync(reqSubCategory, 1);

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
        public async Task UpdateSubCategoryTest()
        {
            Mock<ISubCategoryRepo> mockSubCategoryDAL = new();

            var subCategory = new SubCategory()
            {
                Id = 6,
                CategoryId = 2,
                CreatedAt = DateTime.Now,
                Name = "Teste de título 5",
                SystemDefault = false,
                IconName = "Plate",
                UserId = 2
            };

            SubCategory? subCategoryGetByCategoryIdAndName = null;

            mockSubCategoryDAL.Setup(x => x.GetById(2, 6)).Returns(subCategory);
            mockSubCategoryDAL.Setup(x => x.GetByCategoryIdAndName(2, 2, "Teste de título alterado")).Returns(subCategoryGetByCategoryIdAndName);
            mockSubCategoryDAL.Setup(x => x.UpdateAsync(It.IsAny<SubCategory>())).ReturnsAsync(1);

            SubCategoryService subCategoryBLL = new(mockSubCategoryDAL.Object);


            ReqSubCategory reqSubCategory = new()
            {
                CategoryId = 2,
                Name = "Teste de título alterado",
                IconName = "Plate"
            };

            //SubCategory? nullSubCategory = null;
            // mockSubCategoryDAL.Setup(x => x.UpdateSubCategory(It.IsAny<SubCategory>())).Returns(1);
            // mockSubCategoryDAL.Setup(x => x.GetByCategoryIdAndName(1, reqSubCategory.CategoryId, reqSubCategory.Name)).Returns(nullSubCategory);

            BaseResponse bLLResponse = await subCategoryBLL.UpdateSubCategoryAsync(reqSubCategory, 2, 6);

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
        public async Task Try_UpdateSubCategory_With_Same_Name_Test()
        {
            var subCategory = new SubCategory()
            {
                Id = 6,
                CategoryId = 2,
                CreatedAt = DateTime.Now,
                Name = "Teste de título 5",
                SystemDefault = false,
                IconName = "Plate",
                UserId = 2
            };


            var subCategoryWithSameName = new SubCategory()
            {
                Id = 5,
                CategoryId = 2,
                CreatedAt = DateTime.Now,
                Name = "Teste de título 4",
                SystemDefault = true,
                IconName = "Table",
            };

            Mock<ISubCategoryRepo> mockSubCategoryDAL = new();
            mockSubCategoryDAL.Setup(x => x.GetById(2, 6)).Returns(subCategory);
            mockSubCategoryDAL.Setup(x => x.GetByCategoryIdAndName(2, 2, "Teste de título 4")).Returns(subCategoryWithSameName);


            SubCategoryService subCategoryBLL = new(mockSubCategoryDAL.Object);

            ReqSubCategory reqSubCategory = new()
            {
                CategoryId = 2,
                Name = "Teste de título 4",
            };

            BaseResponse bLLResponse = await subCategoryBLL.UpdateSubCategoryAsync(reqSubCategory, 2, 6);

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
        public async Task Try_Update_A_SystemDefault_SubCategory__Test()
        {

            var subCategory = new SubCategory()
            {
                Id = 5,
                CategoryId = 2,
                CreatedAt = DateTime.Now,
                Name = "Teste de título 4",
                SystemDefault = true,
                IconName = "Table"
            };

            Mock<ISubCategoryRepo> mockSubCategoryDAL = new();

            mockSubCategoryDAL.Setup(x => x.GetById(2, 5)).Returns(subCategory);

            SubCategoryService subCategoryBLL = new(mockSubCategoryDAL.Object);

            ReqSubCategory reqSubCategory = new()
            {
                CategoryId = 2,
                Name = "Teste de título 4",
            };

            BaseResponse bLLResponse = await subCategoryBLL.UpdateSubCategoryAsync(reqSubCategory, 2, 5);

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
        public async Task DeleteSubCategoryTest()
        {
            var subcategory = new SubCategory()
            {
                Id = 6,
                CategoryId = 2,
                CreatedAt = DateTime.Now,
                Name = "Teste de título 5",
                SystemDefault = false,
                IconName = "Plate",
                UserId = 2
            };

            Mock<ISubCategoryRepo> subCategoryDAL = new();

            subCategoryDAL.Setup(x => x.GetById(2, 6)).Returns(subcategory);
            subCategoryDAL.Setup(x => x.UpdateAsync(It.IsAny<SubCategory>())).ReturnsAsync(1);
            SubCategoryService subCategoryBLL = new(subCategoryDAL.Object);

            BaseResponse bLLResponse = await subCategoryBLL.InactiveSubCategoryAsync(2, 6);

            if (bLLResponse?.Error is null && bLLResponse?.Content is null)
                Assert.IsTrue(true);
            else Assert.Fail();
        }

        [TestMethod()]
        public async Task Try_Delete_SystemDefault_SubCategoryTest()
        {
            Mock<ISubCategoryRepo> subCategoryDAL = new();

            var subCategoryById = new SubCategory()
            {
                Id = 5,
                CategoryId = 2,
                CreatedAt = DateTime.Now,
                Name = "Teste de título 4",
                SystemDefault = true,
                IconName = "Table"
            };

            subCategoryDAL.Setup(x => x.GetById(2, 5)).Returns(subCategoryById);

            SubCategoryService subCategoryBLL = new(subCategoryDAL.Object);

            BaseResponse bLLResponse = await subCategoryBLL.InactiveSubCategoryAsync(2, 5);

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