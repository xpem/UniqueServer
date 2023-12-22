using BaseModels;
using InventoryBLL.Interfaces;
using InventoryDAL.Interfaces;
using InventoryModels;
using InventoryModels.Req;
using InventoryModels.Res;

namespace InventoryBLL
{
    public class ItemBLL(IItemSituationDAL itemSituationDAL, ICategoryDAL categoryDAL,
        ISubCategoryDAL subCategoryDAL, IAcquisitionTypeDAL acquisitionTypeDAL,
        IItemDAL itemDAL) : IItemBLL
    {
        public BLLResponse CreateItem(ReqItem reqItem, int uid)
        {
            try
            {
                string? validateError = reqItem.Validate();
                if (!string.IsNullOrEmpty(validateError)) return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = validateError } };

                string? validateIndexes = ValidateIndexes(reqItem, uid);
                if (!string.IsNullOrEmpty(validateIndexes)) return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = validateIndexes } };

                Item item = new()
                {
                    AcquisitionDate = reqItem.AcquisitionDate,
                    AcquisitionTypeId = reqItem.AcquisitionType,
                    CategoryId = reqItem.Category.CategoryId,
                    CreatedAt = DateTime.Now,
                    ItemSituationId = reqItem.Situation,
                    Name = reqItem.Name,
                    UpdatedAt = DateTime.Now,
                    UserId = uid,
                    Comment = reqItem.Comment,
                    PurchaseStore = reqItem.PurchaseStore,
                    PurchaseValue = reqItem.PurchaseValue,
                    ResaleValue = reqItem.ResaleValue,
                    SubCategoryId = reqItem.Category.SubCategory,
                    TechnicalDescription = reqItem.TechnicalDescription,
                    WithdrawalDate = reqItem.WithdrawalDate,
                };

                var resp = itemDAL.Create(item);

                if (resp == 1)
                {
                    var createdCompleteItem = itemDAL.GetById(uid, item.Id);

                    if (createdCompleteItem != null)
                    {
                        ResItem? resItem = BuildResItem(createdCompleteItem);

                        return new BLLResponse { Content = resItem, Error = null };
                    }
                    else throw new Exception($"Não foi possivel recuperar o item de id: {item.Id}");
                }
                else
                    return new BLLResponse { Content = null, Error = new ErrorMessage() { Error = "Não foi possivel adicionar." } };

            }
            catch (Exception) { throw; }
        }

        public BLLResponse DeleteItem(int uid, int id)
        {
            throw new NotImplementedException();
        }

        public BLLResponse Get(int uid)
        {
            throw new NotImplementedException();
        }

        public BLLResponse GetById(int uid, int id)
        {
            Item? item = itemDAL.GetById(uid, id);
            ResItem? resItem = BuildResItem(item);

            return new BLLResponse { Content = resItem, Error = null };
        }

        protected static ResItem? BuildResItem(Item? item)
        {
            ResItem? resItem = null;

            if (item != null)
                resItem = new()
                {
                    Id = item.Id,
                    Category = new ResItemCategory()
                    {
                        Id = item.Category?.Id,
                        Name = item.Category?.Name,
                        Color = item.Category?.Color,
                        SubCategory = new ResItemSubCategory()
                        {
                            Id = item.SubCategory?.Id,
                            Name = item.SubCategory?.Name,
                            IconName = item.SubCategory?.IconName,
                        }
                    },
                    Name = item.Name,
                    AcquisitionDate = item.AcquisitionDate,
                    AcquisitionType = item.AcquisitionTypeId,
                    Comment = item.Comment,
                    CreatedAt = item.CreatedAt,
                    PurchaseStore = item.PurchaseStore,
                    PurchaseValue = item.PurchaseValue,
                    ResaleValue = item.ResaleValue,
                    Situation = new ResItemItemSituation() { Id = item.ItemSituation?.Id, Name = item.ItemSituation?.Name },
                    TechnicalDescription = item.TechnicalDescription,
                    UpdatedAt = item.UpdatedAt,
                    WithdrawalDate = item.WithdrawalDate,
                };

            return resItem;
        }


        public BLLResponse UpdateItem(ReqItem reqItem, int uid)
        {
            throw new NotImplementedException();
        }

        private string? ValidateIndexes(ReqItem reqItem, int uid)
        {
            if (itemSituationDAL.GetById(uid, reqItem.Situation) == null)
                return "Situation with this id don't exist";

            if (categoryDAL.GetById(uid, reqItem.Category.CategoryId) == null)
                return "Category with this id don't exist";

            if ((reqItem.Category.SubCategory is not null) && (subCategoryDAL.GetById(uid, reqItem.Category.SubCategory.Value) == null))
                return "SubCategory with this id don't exist";

            if (acquisitionTypeDAL.GetById(uid, reqItem.AcquisitionType) == null)
                return "Acquisition Type with this id don't exist";

            return null;
        }
    }
}
