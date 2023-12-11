using Microsoft.VisualStudio.TestTools.UnitTesting;
using InventoryBLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryDbContextDAL;
using Microsoft.EntityFrameworkCore;
using Moq;
using InventoryModels;
using InventoryDAL;
using BaseModels;
using InventoryModels.Res;
using InventoryModels.Req;
using InventoryBLL.Interfaces;

namespace InventoryBLL.Tests
{
    [TestClass()]
    public class CategoryBLLTests
    {
        private static Mock<InventoryDbContext> BuildMockInventoryContext()
        {
            Mock<DbSet<Category>> mockSetCategory = new();

            List<Category> categories = [
                new Category()
                {
                    Id = 1,
                    Color = "#bfc9ca",
                    CreatedAt = DateTime.Now,
                    Name = "Casa",
                    UpdatedAt = DateTime.Now,
                    SystemDefault = true,
                    SubCategories = [
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
                    ]
                },
                new Category()
                {
                    Id = 2,
                    Color = "#f5cba7",
                    CreatedAt = DateTime.Now,
                    Name = "Vestimenta",
                    UpdatedAt = DateTime.Now,
                    SystemDefault = true,
                    SubCategories = [
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
                    ]
                },
                new Category()
                {
                    Id = 3,
                    UserId = 1,
                    Color = "#f5cba7",
                    CreatedAt = DateTime.Now,
                    Name = "Teste",
                    UpdatedAt = DateTime.Now,
                    SystemDefault = false,
                    SubCategories = [
                        new SubCategory()
                        {
                            Id = 4,
                            UserId = 1,
                            CategoryId = 2,
                            CreatedAt = DateTime.Now,
                            Name = "Teste de título 6",
                            SystemDefault = false,
                            IconName = "Chair",
                        },
                        new SubCategory()
                        {
                            Id = 5,
                            UserId = 1,
                            CategoryId = 2,
                            CreatedAt = DateTime.Now,
                            Name = "Teste de título 7",
                            SystemDefault = false,
                            IconName = "Table",
                        },
                        new SubCategory()
                        {
                            Id = 6,
                            UserId = 1,
                            CategoryId = 2,
                            CreatedAt = DateTime.Now,
                            Name = "Teste de título 8",
                            SystemDefault = false,
                            IconName = "Plate",
                        },
                    ]
                }
            ];

            IQueryable<Category> data = categories.AsQueryable();

            mockSetCategory.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSetCategory.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSetCategory.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSetCategory.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());

            Mock<InventoryDbContext> mockContext = new();
            mockContext.Setup(m => m.Category).Returns(mockSetCategory.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);
            return mockContext;
        }

        public static CategoryBLL BuildCategoryBLL()
        {
            Mock<InventoryDbContext> mockContext = BuildMockInventoryContext();
            Mock<InventoryDbContext> mockSubCategoryContext = SubCategoryBLLTests.BuildMockInventoryContext();

            CategoryDAL categoryDAL = new(mockContext.Object);
            SubCategoryDAL subCategoryDAL = new(mockSubCategoryContext.Object);
            return new(categoryDAL, subCategoryDAL);
        }

        [TestMethod()]
        public void GetTest()
        {
            CategoryBLL categoryBLL = BuildCategoryBLL();

            BLLResponse bLLResponse = categoryBLL.Get(1);

            if (bLLResponse?.Content is not null)
            {
                List<ResCategory>? categories = bLLResponse.Content as List<ResCategory>;

                Assert.IsTrue(categories?.Count == 3);
                return;
            }

            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateCategoryTest()
        {
            CategoryBLL categoryBLL = BuildCategoryBLL();

            ReqCategory reqCategory = new() { Name = "Teste de título alterado" };

            BLLResponse bLLResponse = categoryBLL.UpdateCategory(reqCategory, 1, 3);

            if (bLLResponse?.Content is not null)
            {
                ResCategoryWithSubCategories? resCategory = bLLResponse?.Content as ResCategoryWithSubCategories;

                if (resCategory != null)
                {
                    Assert.AreEqual("Teste de título alterado", resCategory.Name);
                    return;
                }
                else Assert.Fail();
            }
            else Assert.Fail();
        }

        [TestMethod()]
        public void Try_UpdateCategory_With_Same_Name_Test()
        {
            CategoryBLL categoryBLL = BuildCategoryBLL();

            ReqCategory reqCategory = new() { Name = "Vestimenta" };

            //SubCategory? nullSubCategory = null;
            // mockSubCategoryDAL.Setup(x => x.UpdateSubCategory(It.IsAny<SubCategory>())).Returns(1);
            // mockSubCategoryDAL.Setup(x => x.GetByCategoryIdAndName(1, reqSubCategory.CategoryId, reqSubCategory.Name)).Returns(nullSubCategory);

            BLLResponse bLLResponse = categoryBLL.UpdateCategory(reqCategory, 1, 3);

            if (bLLResponse?.Error is not null)
            {
                ErrorMessage? errorMessage = bLLResponse?.Error as ErrorMessage;

                if (errorMessage?.Error != null)
                    Assert.AreEqual("A Category with this Name has already been added.", errorMessage.Error);
                else Assert.Fail();
            }
            else Assert.Fail();
        }

        [TestMethod()]
        public void DeleteCategoryTest()
        {
            Assert.Fail();
        }
    }
}