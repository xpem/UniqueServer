using InventoryModels.DTOs;
using InventoryRepos.Interfaces;
using InventoryServices.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace InventoryBLLTests
{
    [TestClass]
    public class ItemNameExistsTests
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

        private static bool GetExists(BaseModels.BaseResp resp)
            => (bool)resp.Content!.GetType().GetProperty("exists")!.GetValue(resp.Content)!;

        [TestMethod]
        public async Task CheckItemNameExists_Returns_True_When_Name_Already_Exists()
        {
            int uid = 1;

            Item existingItem = new()
            {
                Id = 5,
                UserId = uid,
                Name = "Notebook Dell",
                AcquisitionDate = new DateOnly(2023, 02, 01),
                ItemSituationId = 1,
                CategoryId = 1,
                AcquisitionTypeId = 1,
                UpdatedAt = DateTime.Now,
                CreatedAt = DateTime.Now,
            };

            Mock<IItemRepo> mockItemDAL = new();
            mockItemDAL.Setup(x => x.GetByNameAsync(uid, "Notebook Dell")).ReturnsAsync(existingItem);

            ItemService itemBLL = BuildService(mockItemDAL);

            BaseModels.BaseResp resp = await itemBLL.CheckItemNameExistsAsync(uid, "Notebook Dell", null);

            Assert.IsTrue(GetExists(resp));
        }

        [TestMethod]
        public async Task CheckItemNameExists_Returns_False_When_Name_Does_Not_Exist()
        {
            int uid = 1;

            Mock<IItemRepo> mockItemDAL = new();
            mockItemDAL.Setup(x => x.GetByNameAsync(uid, "Item Inexistente")).ReturnsAsync((Item?)null);

            ItemService itemBLL = BuildService(mockItemDAL);

            BaseModels.BaseResp resp = await itemBLL.CheckItemNameExistsAsync(uid, "Item Inexistente", null);

            Assert.IsFalse(GetExists(resp));
        }
    }
}
