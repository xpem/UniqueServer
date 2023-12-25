using InventoryBLL;
using InventoryBLLTests.DbContextMocks;
using InventoryDAL.Interfaces;
using InventoryModels;
using InventoryModels.Req;
using InventoryModels.Res;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace InventoryBLL.Tests
{
    [TestClass()]
    public class ItemBLLTests : MockItemsContext
    {
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

        [TestMethod()]
        public void UpdateItemTest()
        {
            int uid = 1;

            ItemBLL itemBLL = MockItemBLL();

            ReqItem reqItem = new()
            {
                AcquisitionDate = new DateOnly(2023, 02, 01),
                AcquisitionType = 2,
                Category = new ReqItemCategory() { CategoryId = 1, SubCategory = 1 },
                Name = "Teste de alteração",
                Situation = 2,
                TechnicalDescription = "teste de descrição técnica",
                Comment = "Teste de comentário",
                PurchaseStore = "Teste de lojal",
                PurchaseValue = 10.0M,
                ResaleValue = 0,
            };

            var resp = itemBLL.UpdateItem(reqItem, uid, 1);

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

        [TestMethod()]
        public void Try_Update_Item_With_Invaild_CategoryId_Test()
        {
            int uid = 1;

            ItemBLL itemBLL = MockItemBLL();

            ReqItem reqItem = new()
            {
                AcquisitionDate = new DateOnly(2023, 02, 01),
                AcquisitionType = 2,
                Category = new ReqItemCategory() { CategoryId = 2, SubCategory = 1 },
                Name = "Teste de alteração",
                Situation = 2,
                TechnicalDescription = "teste de descrição técnica",
                Comment = "Teste de comentário",
                PurchaseStore = "Teste de lojal",
                PurchaseValue = 10.0M,
                ResaleValue = 0,
            };

            var resp = itemBLL.UpdateItem(reqItem, uid, 1);

            if (resp != null && resp.Error != null)
            {
                var errorMsg = resp.Error.Error;
                if (errorMsg != null)
                {
                    Assert.IsTrue(errorMsg == "Category with this id don't exist");
                    return;
                }
            }

            Assert.Fail();
        }
        
        [TestMethod()]
        public void Try_Update_Item_With_Invaild_Id_Test()
        {
            int uid = 1;

            ItemBLL itemBLL = MockItemBLL();

            ReqItem reqItem = new()
            {
                AcquisitionDate = new DateOnly(2023, 02, 01),
                AcquisitionType = 2,
                Category = new ReqItemCategory() { CategoryId = 1, SubCategory = 1 },
                Name = "Teste de alteração",
                Situation = 2,
                TechnicalDescription = "teste de descrição técnica",
                Comment = "Teste de comentário",
                PurchaseStore = "Teste de lojal",
                PurchaseValue = 10.0M,
                ResaleValue = 0,
            };

            var resp = itemBLL.UpdateItem(reqItem, uid, 3);

            if (resp != null && resp.Error != null)
            {
                var errorMsg = resp.Error.Error;
                if (errorMsg != null)
                {
                    Assert.IsTrue(errorMsg == "Invalid id");
                    return;
                }
            }

            Assert.Fail();
        }
    }
}