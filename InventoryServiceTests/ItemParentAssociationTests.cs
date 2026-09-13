using InventoryModels.DTOs;
using InventoryModels.Req;
using InventoryModels.Res.Item;
using InventoryRepos.Interfaces;
using InventoryServices.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryBLLTests
{
    [TestClass]
    public class ItemParentAssociationTests
    {
        private static ItemService BuildService(Mock<IItemRepo> mockItemDAL)
        {
            Mock<IItemSituationRepo> mockItemSituationDAL = new();
            Mock<ICategoryRepo> mockCategoryDAL = new();
            Mock<ISubCategoryRepo> mockSubCategoryDAL = new();
            Mock<IAcquisitionTypeRepo> mockAcquisitionTypeDAL = new();
            Mock<IItemHistoricService> mockItemHistoricService = new();

            return new ItemService(mockItemSituationDAL.Object, mockCategoryDAL.Object,
                mockSubCategoryDAL.Object, mockAcquisitionTypeDAL.Object, mockItemDAL.Object, mockItemHistoricService.Object);
        }

        private static Item BuildBaseItem(int id, int uid, string name, int? parentItemId = null, Item? parentItem = null) => new()
        {
            Id = id,
            UserId = uid,
            Name = name,
            AcquisitionDate = new DateOnly(2024, 01, 01),
            ItemSituationId = 1,
            CategoryId = 1,
            AcquisitionTypeId = 1,
            UpdatedAt = DateTime.Now,
            CreatedAt = DateTime.Now,
            ParentItemId = parentItemId,
            ParentItem = parentItem,
        };

        [TestMethod]
        public async Task CreateItem_With_Valid_ParentItemId_Associates_Successfully()
        {
            int uid = 1;

            Item parentItem = BuildBaseItem(10, uid, "Desktop Gamer");
            // 0 pq o Id não vem preenchido pelo Create mockado
            Item createdItem = BuildBaseItem(0, uid, "SSD Kingston", parentItemId: 10, parentItem: parentItem);

            Mock<IItemRepo> mockItemDAL = new();
            mockItemDAL.Setup(x => x.GetById(uid, 10)).ReturnsAsync(parentItem);
            mockItemDAL.Setup(x => x.GetById(uid, 0)).ReturnsAsync(createdItem);
            mockItemDAL.Setup(x => x.Create(It.IsAny<Item>())).Returns(1);

            ItemService itemBLL = BuildService(mockItemDAL);

            ReqItem reqItem = new()
            {
                Name = "SSD Kingston",
                AcquisitionDate = new DateOnly(2024, 01, 01),
                SituationId = 1,
                AcquisitionType = 1,
                Category = new ReqItemCategory { CategoryId = 1 },
                ParentItemId = 10,
            };

            BaseModels.BaseResp resp = await itemBLL.CreateItem(reqItem, uid);

            Assert.IsTrue(resp.Success);
            ResItem? resItem = resp.Content as ResItem;
            Assert.IsNotNull(resItem);
            Assert.AreEqual(10, resItem!.ParentItem?.Id);
        }

        [TestMethod]
        public async Task CreateItem_With_ParentItem_That_Already_Has_Parent_Returns_Error()
        {
            int uid = 1;

            Item grandParent = BuildBaseItem(20, uid, "Servidor");
            Item parentWithParent = BuildBaseItem(21, uid, "Placa mãe", parentItemId: 20, parentItem: grandParent);

            Mock<IItemRepo> mockItemDAL = new();
            mockItemDAL.Setup(x => x.GetById(uid, 21)).ReturnsAsync(parentWithParent);

            ItemService itemBLL = BuildService(mockItemDAL);

            ReqItem reqItem = new()
            {
                Name = "Cabo Sata",
                AcquisitionDate = new DateOnly(2024, 01, 01),
                SituationId = 1,
                AcquisitionType = 1,
                Category = new ReqItemCategory { CategoryId = 1 },
                ParentItemId = 21,
            };

            BaseModels.BaseResp resp = await itemBLL.CreateItem(reqItem, uid);

            Assert.IsFalse(resp.Success);
            Assert.AreEqual(BaseModels.ErrorCode.InvalidObject, resp.ErrorCode);
        }

        [TestMethod]
        public async Task SetParentItemAsync_Associates_Item_To_Parent_Successfully()
        {
            int uid = 1;

            Item parentItem = BuildBaseItem(10, uid, "Desktop Gamer");
            Item childBefore = BuildBaseItem(30, uid, "Memória RAM");
            Item childAfter = BuildBaseItem(30, uid, "Memória RAM", parentItemId: 10, parentItem: parentItem);

            Mock<IItemRepo> mockItemDAL = new();
            mockItemDAL.Setup(x => x.GetById(uid, 10)).ReturnsAsync(parentItem);
            mockItemDAL.Setup(x => x.GetChildrenAsync(uid, 30)).ReturnsAsync(new List<Item>());
            mockItemDAL.SetupSequence(x => x.GetById(uid, 30))
                .ReturnsAsync(childBefore)
                .ReturnsAsync(childAfter);
            mockItemDAL.Setup(x => x.Update(It.IsAny<Item>())).Returns(1);

            ItemService itemBLL = BuildService(mockItemDAL);

            BaseModels.BaseResp resp = await itemBLL.SetParentItemAsync(uid, 30, 10);

            Assert.IsTrue(resp.Success);
            ResItem? resItem = resp.Content as ResItem;
            Assert.IsNotNull(resItem);
            Assert.AreEqual(10, resItem!.ParentItem?.Id);
        }

        [TestMethod]
        public async Task SetParentItemAsync_Removes_Association_When_ParentItemId_Is_Null()
        {
            int uid = 1;

            Item parentItem = BuildBaseItem(10, uid, "Desktop Gamer");
            Item childWithParent = BuildBaseItem(30, uid, "Memória RAM", parentItemId: 10, parentItem: parentItem);
            Item childWithoutParent = BuildBaseItem(30, uid, "Memória RAM");

            Mock<IItemRepo> mockItemDAL = new();
            mockItemDAL.SetupSequence(x => x.GetById(uid, 30))
                .ReturnsAsync(childWithParent)
                .ReturnsAsync(childWithoutParent);
            mockItemDAL.Setup(x => x.Update(It.IsAny<Item>())).Returns(1);

            ItemService itemBLL = BuildService(mockItemDAL);

            BaseModels.BaseResp resp = await itemBLL.SetParentItemAsync(uid, 30, null);

            Assert.IsTrue(resp.Success);
            ResItem? resItem = resp.Content as ResItem;
            Assert.IsNotNull(resItem);
            Assert.IsNull(resItem!.ParentItem);
        }

        [TestMethod]
        public async Task GetChildrenAsync_Returns_Items_Associated_To_Parent()
        {
            int uid = 1;

            Item child1 = BuildBaseItem(30, uid, "Memória RAM", parentItemId: 10);
            Item child2 = BuildBaseItem(31, uid, "SSD Kingston", parentItemId: 10);

            Mock<IItemRepo> mockItemDAL = new();
            mockItemDAL.Setup(x => x.GetChildrenAsync(uid, 10)).ReturnsAsync(new List<Item> { child1, child2 });

            ItemService itemBLL = BuildService(mockItemDAL);

            BaseModels.BaseResp resp = await itemBLL.GetChildrenAsync(uid, 10);

            List<ResItem>? resItems = resp.Content as List<ResItem>;
            Assert.IsNotNull(resItems);
            Assert.AreEqual(2, resItems!.Count);
        }

        [TestMethod]
        public async Task DeleteItem_Detaches_Children_From_Deleted_Parent()
        {
            int uid = 1;

            Item parentItem = BuildBaseItem(10, uid, "Desktop Gamer");

            Mock<IItemRepo> mockItemDAL = new();
            mockItemDAL.Setup(x => x.GetById(uid, 10)).ReturnsAsync(parentItem);
            mockItemDAL.Setup(x => x.Inactivate(uid, 10)).Returns(1);
            mockItemDAL.Setup(x => x.DetachChildrenAsync(uid, 10)).ReturnsAsync(2);

            ItemService itemBLL = BuildService(mockItemDAL);

            BaseModels.BaseResp resp = await itemBLL.DeleteItem(uid, 10, System.IO.Path.GetTempPath());

            Assert.IsTrue(resp.Success);
            mockItemDAL.Verify(x => x.DetachChildrenAsync(uid, 10), Times.Once);
        }
    }
}
