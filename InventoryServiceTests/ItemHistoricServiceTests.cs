using InventoryModels.DTOs;
using InventoryRepos.Interfaces;
using InventoryServices.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace InventoryBLLTests
{
    [TestClass]
    public class ItemHistoricServiceTests
    {
        private static Item BaseItem() => new()
        {
            Id = 1,
            UserId = 1,
            Name = "Item Teste",
            CategoryId = 1,
            Category = new Category { Id = 1, Name = "Casa", Color = "#bfc9ca", CreatedAt = DateTime.UtcNow },
            SubCategoryId = 1,
            SubCategory = new SubCategory { Id = 1, Name = "Móveis", CategoryId = 1, CreatedAt = DateTime.UtcNow },
            ItemSituationId = 1,
            ItemSituation = new ItemSituation { Id = 1, Name = "Em uso", CreatedAt = DateTime.UtcNow, Sequence = 1, Type = SituationType.In },
            AcquisitionTypeId = 1,
            AcquisitionType = new AcquisitionType { Id = 1, Name = "Compra", CreatedAt = DateTime.UtcNow, Sequence = 1 },
            AcquisitionDate = new DateOnly(2024, 1, 1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        [TestMethod]
        public async Task BuildAndCreate_NoChanges_DoesNotCallRepo()
        {
            var repo = new Mock<IItemHistoricRepo>();
            var service = new ItemHistoricService(repo.Object);
            var item = BaseItem();

            await service.BuildAndCreateItemUpdateHistoricAsync(item, item);

            repo.Verify(r => r.AddAsync(It.IsAny<ItemHistoric>()), Times.Never);
            repo.Verify(r => r.AddRangeItemListAsync(It.IsAny<List<ItemHistoricItem>>()), Times.Never);
        }

        [TestMethod]
        public async Task BuildAndCreate_NameChanged_CreatesHistoricWithTypeId2()
        {
            var repo = new Mock<IItemHistoricRepo>();
            ItemHistoric? captured = null;
            repo.Setup(r => r.AddAsync(It.IsAny<ItemHistoric>()))
                .Callback<ItemHistoric>(h => captured = h)
                .ReturnsAsync(1);
            repo.Setup(r => r.AddRangeItemListAsync(It.IsAny<List<ItemHistoricItem>>()))
                .ReturnsAsync(1);

            var service = new ItemHistoricService(repo.Object);
            var oldItem = BaseItem();
            var newItem = BaseItem();
            newItem.Name = "Nome Alterado";

            await service.BuildAndCreateItemUpdateHistoricAsync(oldItem, newItem);

            Assert.IsNotNull(captured);
            Assert.AreEqual(2, captured.ItemHistoricTypeId);
        }

        [TestMethod]
        public async Task BuildAndCreate_NameChanged_CreatesItemHistoricItemWithFieldId1()
        {
            var repo = new Mock<IItemHistoricRepo>();
            List<ItemHistoricItem>? capturedItems = null;
            repo.Setup(r => r.AddAsync(It.IsAny<ItemHistoric>())).ReturnsAsync(1);
            repo.Setup(r => r.AddRangeItemListAsync(It.IsAny<List<ItemHistoricItem>>()))
                .Callback<List<ItemHistoricItem>>(items => capturedItems = items)
                .ReturnsAsync(1);

            var service = new ItemHistoricService(repo.Object);
            var oldItem = BaseItem();
            var newItem = BaseItem();
            newItem.Name = "Nome Alterado";

            await service.BuildAndCreateItemUpdateHistoricAsync(oldItem, newItem);

            Assert.IsNotNull(capturedItems);
            Assert.AreEqual(1, capturedItems.Count);
            Assert.AreEqual(1, capturedItems[0].ItemHistoricItemFieldId);
            Assert.AreEqual("Item Teste", capturedItems[0].UpdatedFrom);
            Assert.AreEqual("Nome Alterado", capturedItems[0].UpdatedTo);
        }

        [TestMethod]
        public async Task BuildAndCreate_CategoryChanged_CreatesItemHistoricItemWithFieldId2()
        {
            var repo = new Mock<IItemHistoricRepo>();
            List<ItemHistoricItem>? capturedItems = null;
            repo.Setup(r => r.AddAsync(It.IsAny<ItemHistoric>())).ReturnsAsync(1);
            repo.Setup(r => r.AddRangeItemListAsync(It.IsAny<List<ItemHistoricItem>>()))
                .Callback<List<ItemHistoricItem>>(items => capturedItems = items)
                .ReturnsAsync(1);

            var service = new ItemHistoricService(repo.Object);
            var oldItem = BaseItem();
            var newItem = BaseItem();
            newItem.CategoryId = 2;
            newItem.Category = new Category { Id = 2, Name = "Vestimenta", Color = "#f5cba7", CreatedAt = DateTime.UtcNow };

            await service.BuildAndCreateItemUpdateHistoricAsync(oldItem, newItem);

            Assert.IsNotNull(capturedItems);
            Assert.AreEqual(1, capturedItems.Count);
            Assert.AreEqual(2, capturedItems[0].ItemHistoricItemFieldId);
        }

        [TestMethod]
        public async Task BuildAndCreate_MultipleFieldsChanged_CreatesOneItemPerChangedField()
        {
            var repo = new Mock<IItemHistoricRepo>();
            List<ItemHistoricItem>? capturedItems = null;
            repo.Setup(r => r.AddAsync(It.IsAny<ItemHistoric>())).ReturnsAsync(1);
            repo.Setup(r => r.AddRangeItemListAsync(It.IsAny<List<ItemHistoricItem>>()))
                .Callback<List<ItemHistoricItem>>(items => capturedItems = items)
                .ReturnsAsync(1);

            var service = new ItemHistoricService(repo.Object);
            var oldItem = BaseItem();
            var newItem = BaseItem();
            newItem.Name = "Nome Novo";
            newItem.ItemSituationId = 2;
            newItem.ItemSituation = new ItemSituation { Id = 2, Name = "Guardado", CreatedAt = DateTime.UtcNow, Sequence = 2, Type = SituationType.In };
            newItem.PurchaseValue = 500M;

            await service.BuildAndCreateItemUpdateHistoricAsync(oldItem, newItem);

            Assert.IsNotNull(capturedItems);
            Assert.AreEqual(3, capturedItems.Count);

            var fieldIds = capturedItems.Select(i => i.ItemHistoricItemFieldId).ToList();
            CollectionAssert.Contains(fieldIds, 1); // Nome
            CollectionAssert.Contains(fieldIds, 4); // Situação
            CollectionAssert.Contains(fieldIds, 8); // Valor de Compra
        }
    }
}
