using InventoryBLL;
using InventoryModels.DTOs;
using InventoryRepos.Interfaces;
using Moq;

namespace InventoryBLLTests.DbContextMocks
{
    public class MockItemsContext
    {
        protected static ItemService MockItemBLL()
        {
            ItemSituation itemSituation = new()
            {
                CreatedAt = DateTime.Now,
                Name = "Teste de situação",
                Sequence = 1,
                Id = 1,
                SystemDefault = true,
                UpdatedAt = DateTime.Now,
                UserId = 1,
                Type = SituationType.In,

            };

            ItemSituation itemSituation2 = new()
            {
                CreatedAt = DateTime.Now,
                Name = "Teste 2 de situação",
                Sequence = 2,
                Id = 2,
                SystemDefault = true,
                UpdatedAt = DateTime.Now,
                UserId = 1,
                Type = SituationType.Out,
            };

            AcquisitionType acquisitionType = new()
            {
                CreatedAt = DateTime.Now,
                Name = "Teste de Tipo de entrada",
                Sequence = 1,
                Id = 1,
                SystemDefault = true,
                UpdatedAt = DateTime.Now,
            };
            AcquisitionType acquisitionType2 = new()
            {
                CreatedAt = DateTime.Now,
                Name = "Teste 2 de Tipo de entrada",
                Sequence = 2,
                Id = 2,
                SystemDefault = true,
                UpdatedAt = DateTime.Now,
            };

            SubCategory subCategory = new()
            {
                Id = 1,
                CategoryId = 1,
                CreatedAt = DateTime.Now,
                Name = "Teste de título 1",
                SystemDefault = true,
                IconName = "Dog",
            };

            Category category = new()
            {
                Id = 1,
                Color = "#bfc9ca",
                CreatedAt = DateTime.Now,
                Name = "Casa",
                UpdatedAt = DateTime.Now,
                SystemDefault = true,
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
                AcquisitionType = acquisitionType,
                Id = 1,
                ItemSituation = itemSituation,
                SubCategory = subCategory,
                Category = category,
                Name = "Teste",
                TechnicalDescription = "teste de descrição técnica",
                Comment = "Teste de comentário",
                PurchaseStore = "Teste de lojal",
                PurchaseValue = 10.0M,
                ResaleValue = 0,
            };

            Item UpdatedItem = new()
            {
                AcquisitionDate = new DateOnly(2023, 02, 01),
                AcquisitionTypeId = 1,
                CategoryId = 1,
                SubCategoryId = 1,
                CreatedAt = DateTime.Now,
                ItemSituationId = 1,
                UpdatedAt = DateTime.Now,
                UserId = 1,
                AcquisitionType = acquisitionType2,
                Id = 1,
                ItemSituation = itemSituation2,
                SubCategory = subCategory,
                Category = category,
                Name = "Teste de alteração",
                TechnicalDescription = "teste de descrição técnica",
                Comment = "Teste de comentário",
                PurchaseStore = "Teste de lojal",
                PurchaseValue = 10.0M,
                ResaleValue = 0,
            };

            Mock<IItemRepo> mockItemDAL = new();
            Mock<IItemSituationRepo> mockItemSituationDAL = new();
            Mock<ICategoryRepo> mockCategoryDAL = new();
            Mock<ISubCategoryRepo> mockSubCategoryDAL = new();
            Mock<IAcquisitionTypeRepo> mockAcquisitionTypeDAL = new();

            mockSubCategoryDAL.Setup(x => x.GetById(1, 1)).Returns(subCategory);
            mockCategoryDAL.Setup(x => x.GetById(1, 1)).Returns(category);
            mockAcquisitionTypeDAL.Setup(x => x.GetById(1, 1)).Returns(acquisitionType);
            mockAcquisitionTypeDAL.Setup(x => x.GetById(1, 2)).Returns(acquisitionType2);
            mockItemSituationDAL.Setup(x => x.GetById(1, 1)).Returns(itemSituation);
            mockItemSituationDAL.Setup(x => x.GetById(1, 2)).Returns(itemSituation2);

            //0 pq n tem o id retornado pelo add no dal mockado.
            mockItemDAL.Setup(x => x.GetById(1, 0)).Returns(item);

            mockItemDAL.SetupSequence(x => x.GetById(1, 1)).Returns(item).Returns(UpdatedItem);
            mockItemDAL.Setup(x => x.Create(It.IsAny<Item>())).Returns(1);
            mockItemDAL.Setup(x => x.Update(It.IsAny<Item>())).Returns(1);
            return new ItemService(mockItemSituationDAL.Object, mockCategoryDAL.Object,
                mockSubCategoryDAL.Object, mockAcquisitionTypeDAL.Object, mockItemDAL.Object);
        }


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
    }
}
