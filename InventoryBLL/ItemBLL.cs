using BaseModels;
using InventoryDAL.Interfaces;
using InventoryModels;
using InventoryModels.Req;

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
                    CategoryId = reqItem.Category.Category,
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
                    
                    //ResCategory resCategory = new()
                    //{
                    //    Name = category.Name,
                    //    Color = category.Color,
                    //    SystemDefault = category.SystemDefault,
                    //    Id = category.Id
                    //};
                    return new BLLResponse { Content = resCategory, Error = null };
                }
                else
                    return new BLLResponse { Content = null, Error = new ErrorMessage() { Error = "Não foi possivel adicionar." } };

            }
            catch (Exception ex) { throw; }
        }

        public string? ValidateIndexes(ReqItem reqItem, int uid)
        {
            if (itemSituationDAL.GetById(uid, reqItem.Situation) == null)
                return "Situation with this id don't exist";

            if (categoryDAL.GetById(uid, reqItem.Category.Category) == null)
                return "Category with this id don't exist";

            if ((reqItem.Category.SubCategory is not null) && (subCategoryDAL.GetById(uid, reqItem.Category.SubCategory.Value) == null))
                return "SubCategory with this id don't exist";

            if (acquisitionTypeDAL.GetById(uid, reqItem.AcquisitionType) == null)
                return "Acquisition Type with this id don't exist";

            return null;
        }
    }
}
