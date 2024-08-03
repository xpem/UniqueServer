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
    public class CategoryBLLTests
    {
        [TestMethod()]
        public void GetTest()
        {
            List<Category> categories = [
                new Category()
                {
                    Id = 1,
                    Color = "#bfc9ca",
                    UserId = 1,
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
                    UserId = 1,
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

            Mock<ICategoryDAL> categoryDAL = new();
            Mock<ISubCategoryDAL> subCategoryDAL = new();

            CategoryBLL categoryBLL = new(categoryDAL.Object, subCategoryDAL.Object);

            categoryDAL.Setup(x => x.Get(1)).Returns(categories);

            BaseResponse bLLResponse = categoryBLL.Get(1);

            if (bLLResponse?.Content is not null)
            {
                List<ResCategory>? resCategories = bLLResponse.Content as List<ResCategory>;

                Assert.IsTrue(resCategories?.Count == 3);
                return;
            }

            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateCategoryTest()
        {
            Mock<ICategoryDAL> categoryDAL = new();
            Mock<ISubCategoryDAL> subCategoryDAL = new();

            CategoryBLL categoryBLL = new(categoryDAL.Object, subCategoryDAL.Object);

            var category = new Category()
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
            };

            categoryDAL.Setup(x => x.GetById(1, 3)).Returns(category);

            categoryDAL.Setup(x => x.Update(It.IsAny<Category>())).Returns(1);

            ReqCategory reqCategory = new() { Name = "Teste de título alterado" };

            BaseResponse bLLResponse = categoryBLL.UpdateCategory(reqCategory, 1, 3);

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
            ReqCategory reqCategory = new() { Name = "Vestimenta" };

            Mock<ICategoryDAL> categoryDAL = new();
            Mock<ISubCategoryDAL> subCategoryDAL = new();

            var category = new Category()
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
            };

            var categoryByName = new Category()
            {
                Id = 2,
                UserId = 1,
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
            };

            categoryDAL.Setup(x => x.GetById(1, 3)).Returns(category);
            categoryDAL.Setup(x => x.GetByName(1, "Vestimenta")).Returns(categoryByName);

            CategoryBLL categoryBLL = new(categoryDAL.Object, subCategoryDAL.Object);

            BaseResponse bLLResponse = categoryBLL.UpdateCategory(reqCategory, 1, 3);

            if (bLLResponse?.Error is not null)
            {
                Error? errorMessage = bLLResponse?.Error;

                if (errorMessage?.Message != null)
                    Assert.AreEqual("A Category with this Name has already been added.", errorMessage.Message);
                else Assert.Fail();
            }
            else Assert.Fail();
        }
    }
}