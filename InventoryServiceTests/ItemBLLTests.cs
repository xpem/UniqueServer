using InventoryBLLTests.DbContextMocks;
using InventoryModels.DTOs;
using InventoryModels.Req;
using InventoryModels.Res.Item;
using InventoryRepos.Interfaces;
using InventoryServices.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace InventoryBLLTests
{
    [TestClass()]
    public class ItemBLLTests : MockItemsContext
    {
        [TestMethod()]
        public async Task CreateItemTest()
        {
            int uid = 1;

            ReqItem reqItem = new()
            {
                AcquisitionDate = new DateOnly(2023, 02, 01),
                AcquisitionType = 1,
                Category = new ReqItemCategory() { CategoryId = 1, SubCategoryId = 1 },
                Name = "Teste",
                SituationId = 1,
                TechnicalDescription = "teste de descrição técnica",
                Comment = "Teste de comentário",
                PurchaseStore = "Teste de lojal",
                PurchaseValue = 10.0M,
                ResaleValue = 0,
            };

            ItemService itemBLL = MockItemBLL();

            BaseModels.BaseResp resp = await itemBLL.CreateItem(reqItem, uid);

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
        public async Task GetByIdTest()
        {
            int uid = 1;

            ItemService itemBLL = MockItemBLL();

            BaseModels.BaseResp resp = await itemBLL.GetById(uid, 1);

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
        public async Task UpdateItemTest()
        {
            int uid = 1;

            ItemService itemBLL = MockItemBLL();

            ReqItem reqItem = new()
            {
                AcquisitionDate = new DateOnly(2023, 02, 01),
                AcquisitionType = 2,
                Category = new ReqItemCategory() { CategoryId = 1, SubCategoryId = 1 },
                Name = "Teste de alteração",
                SituationId = 2,
                TechnicalDescription = "teste de descrição técnica",
                Comment = "Teste de comentário",
                PurchaseStore = "Teste de lojal",
                PurchaseValue = 10.0M,
                ResaleValue = 0,
            };

            BaseModels.BaseResp resp = await itemBLL.UpdateItem(reqItem, uid, 1);

            if (resp != null && resp.Content != null)
            {
                ResItem? resItem = resp.Content as ResItem;

                if (resItem != null)
                {
                    Assert.IsTrue(resItem.Name == "Teste de alteração");
                    return;
                }
            }

            Assert.Fail();
        }

        //[TestMethod()]
        //public async Task Try_Update_Item_With_Invaild_CategoryId_Test()
        //{
        //    int uid = 1;

        //    ItemService itemBLL = MockItemBLL();

        //    ReqItem reqItem = new()
        //    {
        //        AcquisitionDate = new DateOnly(2023, 02, 01),
        //        AcquisitionType = 2,
        //        Category = new ReqItemCategory() { CategoryId = 2, SubCategoryId = 1 },
        //        Name = "Teste de alteração",
        //        SituationId = 2,
        //        TechnicalDescription = "teste de descrição técnica",
        //        Comment = "Teste de comentário",
        //        PurchaseStore = "Teste de lojal",
        //        PurchaseValue = 10.0M,
        //        ResaleValue = 0,
        //    };

        //    BaseModels.BaseResponse resp = await itemBLL.UpdateItem(reqItem, uid, 1);

        //    if (resp != null && resp.Error != null)
        //    {
        //        string errorMsg = resp.Error.Message;
        //        if (errorMsg != null)
        //        {
        //            Assert.IsTrue(errorMsg == "Category with this id don't exist");
        //            return;
        //        }
        //    }

        //    Assert.Fail();
        //}

        [TestMethod()]
        public async Task Try_Update_Item_With_Invaild_Id_Test()
        {
            int uid = 1;

            ItemService itemBLL = MockItemBLL();

            ReqItem reqItem = new()
            {
                AcquisitionDate = new DateOnly(2023, 02, 01),
                AcquisitionType = 2,
                Category = new ReqItemCategory() { CategoryId = 1, SubCategoryId = 1 },
                Name = "Teste de alteração",
                SituationId = 2,
                TechnicalDescription = "teste de descrição técnica",
                Comment = "Teste de comentário",
                PurchaseStore = "Teste de lojal",
                PurchaseValue = 10.0M,
                ResaleValue = 0,
            };

            BaseModels.BaseResp resp = await itemBLL.UpdateItem(reqItem, uid, 3);

            if (resp != null && resp.Error != null)
            {
                string errorMsg = resp.Error.Message;
                if (errorMsg != null)
                {
                    Assert.IsTrue(errorMsg == "Invalid id");
                    return;
                }
            }

            Assert.Fail();
        }

        [TestMethod()]
        public async Task UpdateItem_Preserves_Existing_Image1_And_Image2()
        {
            int uid = 1;

            Item oldItem = new()
            {
                Id = 1,
                UserId = uid,
                Name = "Notebook",
                AcquisitionDate = new DateOnly(2023, 01, 01),
                ItemSituationId = 1,
                CategoryId = 1,
                AcquisitionTypeId = 1,
                UpdatedAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                Image1 = "foto1.jpg",
                Image2 = "foto2.jpg",
            };

            Item updatedItem = new()
            {
                Id = 1,
                UserId = uid,
                Name = "Notebook Atualizado",
                AcquisitionDate = new DateOnly(2023, 01, 01),
                ItemSituationId = 1,
                CategoryId = 1,
                AcquisitionTypeId = 1,
                UpdatedAt = DateTime.Now,
                CreatedAt = oldItem.CreatedAt,
                Image1 = "foto1.jpg",
                Image2 = "foto2.jpg",
            };

            Item? capturedItem = null;

            Mock<IItemRepo> mockItemDAL = new();
            mockItemDAL.SetupSequence(x => x.GetById(uid, 1))
                .ReturnsAsync(oldItem)
                .ReturnsAsync(updatedItem);
            mockItemDAL.Setup(x => x.Update(It.IsAny<Item>()))
                .Callback<Item>(item => capturedItem = item)
                .Returns(1);

            Mock<IItemSituationRepo> mockItemSituationDAL = new();
            Mock<ICategoryRepo> mockCategoryDAL = new();
            Mock<ISubCategoryRepo> mockSubCategoryDAL = new();
            Mock<IAcquisitionTypeRepo> mockAcquisitionTypeDAL = new();
            Mock<IItemHistoricService> mockItemHistoricService = new();

            ItemService itemBLL = new(mockItemSituationDAL.Object, mockCategoryDAL.Object,
                mockSubCategoryDAL.Object, mockAcquisitionTypeDAL.Object, mockItemDAL.Object, mockItemHistoricService.Object);

            ReqItem reqItem = new()
            {
                Name = "Notebook Atualizado",
                AcquisitionDate = new DateOnly(2023, 01, 01),
                SituationId = 1,
                AcquisitionType = 1,
                Category = new ReqItemCategory { CategoryId = 1 },
            };

            BaseModels.BaseResp resp = await itemBLL.UpdateItem(reqItem, uid, 1);

            Assert.IsTrue(resp.Success);
            Assert.IsNotNull(capturedItem);
            Assert.AreEqual("foto1.jpg", capturedItem!.Image1);
            Assert.AreEqual("foto2.jpg", capturedItem.Image2);
        }
    }
}