using InventoryBLL;
using InventoryDAL.Interfaces;
using InventoryModels;
using InventoryModels.Req;
using InventoryModels.Res;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace InventoryBLL.Tests
{
    [TestClass()]
    public class ItemBLLTests
    {
        #region BuildMockInventoryContext

        //public static Mock<InventoryDbContext> BuildMockInventoryContext()
        //{
        //    Mock<DbSet<ItemSituation>> mockSetItemSituation = new();
        //    Mock<InventoryDbContext> mockContext = new();

        //    List<ItemSituation> itemSituations =
        //    [
        //        new ItemSituation() { Id = 1, Name = "teste a", CreatedAt = DateTime.Now, Sequence = 1 },
        //        new ItemSituation() { Id = 2, Name = "teste b", CreatedAt = DateTime.Now, Sequence = 2 },
        //        new ItemSituation() { Id = 3, Name = "teste c", CreatedAt = DateTime.Now, Sequence = 3 },
        //    ];

        //    IQueryable<ItemSituation> dataItemSituation = itemSituations.AsQueryable();

        //    mockSetItemSituation.As<IQueryable<ItemSituation>>().Setup(m => m.Provider).Returns(dataItemSituation.Provider);
        //    mockSetItemSituation.As<IQueryable<ItemSituation>>().Setup(m => m.Expression).Returns(dataItemSituation.Expression);
        //    mockSetItemSituation.As<IQueryable<ItemSituation>>().Setup(m => m.ElementType).Returns(dataItemSituation.ElementType);
        //    mockSetItemSituation.As<IQueryable<ItemSituation>>().Setup(m => m.GetEnumerator()).Returns(() => dataItemSituation.GetEnumerator());
        //    mockContext.Setup(m => m.ItemSituation).Returns(mockSetItemSituation.Object);
        //    // mockContext.Setup(m => m.SaveChanges()).Returns(1);

        //    Mock<DbSet<SubCategory>> mockSetSubCategory = new();

        //    List<SubCategory> SubCategories = [
        //        new SubCategory()
        //        {
        //            Id = 1,
        //            CategoryId = 1,
        //            CreatedAt = DateTime.Now,
        //            Name = "Teste de título 1",
        //            SystemDefault = true,
        //            IconName = "Dog",
        //        },
        //        new SubCategory()
        //        {
        //            Id = 2,
        //            CategoryId = 1,
        //            CreatedAt = DateTime.Now,
        //            Name = "Teste de título 2",
        //            SystemDefault = true,
        //            IconName = "Cat",
        //        },
        //    ];

        //    IQueryable<SubCategory> dataSubCategory = SubCategories.AsQueryable();

        //    mockSetSubCategory.As<IQueryable<SubCategory>>().Setup(m => m.Provider).Returns(dataSubCategory.Provider);
        //    mockSetSubCategory.As<IQueryable<SubCategory>>().Setup(m => m.Expression).Returns(dataSubCategory.Expression);
        //    mockSetSubCategory.As<IQueryable<SubCategory>>().Setup(m => m.ElementType).Returns(dataSubCategory.ElementType);
        //    mockSetSubCategory.As<IQueryable<SubCategory>>().Setup(m => m.GetEnumerator()).Returns(() => dataSubCategory.GetEnumerator());
        //    mockContext.Setup(m => m.SubCategory).Returns(mockSetSubCategory.Object);
        //    // mockContext.Setup(m => m.SaveChanges()).Returns(1);
        //}

        #endregion

        protected static ItemBLL MockItemBLL()
        {
            ItemSituation itemSituation = new ItemSituation()
            {
                CreatedAt = DateTime.Now,
                Name = "Teste de situação",
                Sequence = 1,
                Id = 1,
                SystemDefault = true,
                UpdatedAt = DateTime.Now,
                UserId = 1
            };

            AcquisitionType acquisitionType = new AcquisitionType()
            {
                CreatedAt = DateTime.Now,
                Name = "Teste de Tipo de entrada",
                Sequence = 1,
                Id = 1,
                SystemDefault = true,
                UpdatedAt = DateTime.Now
            };
            SubCategory subCategory = new SubCategory()
            {
                Id = 1,
                CategoryId = 1,
                CreatedAt = DateTime.Now,
                Name = "Teste de título 1",
                SystemDefault = true,
                IconName = "Dog",
            };

            Category category = new Category()
            {
                Id = 1,
                Color = "#bfc9ca",
                CreatedAt = DateTime.Now,
                Name = "Casa",
                UpdatedAt = DateTime.Now,
                SystemDefault = true
            };

            Item item = new()
            {
                AcquisitionDate = new DateOnly(2023, 02, 01),
                AcquisitionTypeId = 1,
                CategoryId = 1,
                SubCategoryId = 1,
                CreatedAt = DateTime.Now,
                ItemSituationId = 1,
                UpdatedAt = DateTime.Now,
                UserId = 1,
                AcquisitionType = new AcquisitionType()
                {
                    CreatedAt = DateTime.Now,
                    Name = "Teste de Tipo de entrada",
                    Sequence = 1,
                    Id = 1,
                    SystemDefault = true,
                    UpdatedAt = DateTime.Now
                },
                Id = 1,
                ItemSituation = new ItemSituation()
                {
                    CreatedAt = DateTime.Now,
                    Name = "Teste de situação",
                    Sequence = 1,
                    Id = 1,
                    SystemDefault = true,
                    UpdatedAt = DateTime.Now,
                    UserId = 1
                },
                SubCategory = new SubCategory()
                {
                    Id = 1,
                    CategoryId = 1,
                    CreatedAt = DateTime.Now,
                    Name = "Teste de título 1",
                    SystemDefault = true,
                    IconName = "Dog",
                },
                Category = new Category()
                {
                    Id = 1,
                    Color = "#bfc9ca",
                    CreatedAt = DateTime.Now,
                    Name = "Casa",
                    UpdatedAt = DateTime.Now,
                    SystemDefault = true
                },
                Name = "Teste",
                TechnicalDescription = "teste de descrição técnica",
                Comment = "Teste de comentário",
                PurchaseStore = "Teste de lojal",
                PurchaseValue = 10.0M,
                ResaleValue = 0,

            };

            Mock<IItemDAL> mockItemDAL = new();
            Mock<IItemSituationDAL> mockItemSituationDAL = new();
            Mock<ICategoryDAL> mockCategoryDAL = new Mock<ICategoryDAL>();
            Mock<ISubCategoryDAL> mockSubCategoryDAL = new Mock<ISubCategoryDAL>();
            Mock<IAcquisitionTypeDAL> mockAcquisitionTypeDAL = new Mock<IAcquisitionTypeDAL>();

            mockSubCategoryDAL.Setup(x => x.GetById(1, 1)).Returns(subCategory);
            mockCategoryDAL.Setup(x => x.GetById(1, 1)).Returns(category);
            mockAcquisitionTypeDAL.Setup(x => x.GetById(1, 1)).Returns(acquisitionType);
            mockItemSituationDAL.Setup(x => x.GetById(1, 1)).Returns(itemSituation);
            
            //0 pq n tem o id retornado pelo add no dal mockado.
            mockItemDAL.Setup(x => x.GetById(1, 0)).Returns(item);

            mockItemDAL.Setup(x => x.GetById(1, 1)).Returns(item);
            mockItemDAL.Setup(x => x.Create(It.IsAny<Item>())).Returns(1);

            return new ItemBLL(mockItemSituationDAL.Object, mockCategoryDAL.Object,
                mockSubCategoryDAL.Object, mockAcquisitionTypeDAL.Object, mockItemDAL.Object);
        }

        [TestMethod()]
        public void CreateItemTest()
        {
            int uid = 1;

            ReqItem reqItem = new()
            {
                AcquisitionDate = new DateOnly(2023, 02, 01),
                AcquisitionType = 1,
                Category = new ReqItemCategory() { CategoryId = 1, SubCategory = 1 },
                Name = "Teste",
                Situation = 1,
                TechnicalDescription = "teste de descrição técnica",
                Comment = "Teste de comentário",
                PurchaseStore = "Teste de lojal",
                PurchaseValue = 10.0M,
                ResaleValue = 0,
            };

            ItemBLL itemBLL = MockItemBLL();

            var resp = itemBLL.CreateItem(reqItem, uid);

            if (resp != null && resp.Content != null)
            {
                ResItem? resItem = resp.Content as ResItem;

                if (resItem != null)
                {
                    Assert.IsTrue(resItem.Id == 1);
                    return;
                }
            }

            Assert.Fail();
        }

        [TestMethod()]
        public void GetByIdTest()
        {
            int uid = 1;

            ItemBLL itemBLL = MockItemBLL();

            var resp = itemBLL.GetById(uid, 1);

            if (resp != null && resp.Content != null)
            {
                ResItem? resItem = resp.Content as ResItem;

                if (resItem != null)
                {
                    Assert.IsTrue(resItem.Id == 1);
                    return;
                }
            }

            Assert.Fail();
        }
    }
}