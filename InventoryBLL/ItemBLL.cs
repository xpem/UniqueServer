using BaseModels;
using InventoryBLL.Interfaces;
using InventoryDAL;
using InventoryDAL.Interfaces;
using InventoryModels;
using InventoryModels.Req;
using InventoryModels.Res;
using System.IO;

namespace InventoryBLL
{
    public class ItemBLL(IItemSituationDAL itemSituationDAL, ICategoryDAL categoryDAL,
        ISubCategoryDAL subCategoryDAL, IAcquisitionTypeDAL acquisitionTypeDAL,
        IItemDAL itemDAL) : IItemBLL
    {
        readonly int pageSize = 10;

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
                    ItemSituationId = reqItem.SituationId,
                    Name = reqItem.Name,
                    UpdatedAt = DateTime.Now,
                    UserId = uid,
                    Comment = reqItem.Comment,
                    PurchaseStore = reqItem.PurchaseStore,
                    PurchaseValue = reqItem.PurchaseValue,
                    ResaleValue = reqItem.ResaleValue,
                    SubCategoryId = reqItem.Category.SubCategoryId,
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

        public BLLResponse DeleteItem(int uid, int id, string filePath)
        {
            Item? item = itemDAL.GetById(uid, id);

            if (item == null)
                return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = "Invalid id" } };

            string? fileName1 = null, fileName2 = null;

            if (item.Image1 != null) fileName1 = item.Image1;
            if (item.Image2 != null) fileName2 = item.Image2;

            var respExec = itemDAL.Delete(item);

            if (respExec == 1)
            {
                if (fileName1 != null)
                    System.IO.File.Delete(Path.Combine(filePath, fileName1));

                if (fileName2 != null)
                    System.IO.File.Delete(Path.Combine(filePath, fileName2));

                return new BLLResponse { };
            }
            else
                return new BLLResponse { Content = null, Error = new ErrorMessage() { Error = "Não foi possivel excluir." } };
        }

        public async Task<BLLResponse> GetAsync(int uid, int page)
        {
            if (page <= 0)
                return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = "Invalid page" } };

            List<Item>? items = await itemDAL.GetAsync(uid, page, pageSize);
            List<ResItem> resItems = [];

            if (items != null && items.Count > 0)
                foreach (var item in items)
                {
                    var bildedResItem = BuildResItem(item);

                    if (bildedResItem != null)
                        resItems.Add(bildedResItem);
                }
            return new BLLResponse { Content = resItems };
        }

        public async Task<BLLResponse> GetTotalItemsPagesAsync(int uid)
        {
            int totalItems = await itemDAL.GetTotalAsync(uid);

            double fractionalTotalPages = (double)totalItems / (double)pageSize;

            if (!(fractionalTotalPages % 1 == 0)) fractionalTotalPages += 1;

            int totalPage = Convert.ToInt32(Math.Round(fractionalTotalPages, 0, MidpointRounding.ToZero));

            ResTotalItems resTotalItems = new ResTotalItems() { TotalItems = totalItems, TotalPages = totalPage };

            return new BLLResponse { Content = resTotalItems };
        }

        public BLLResponse GetById(int uid, int id)
        {
            Item? item = itemDAL.GetById(uid, id);

            if (item == null)
                return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = "Invalid id" } };

            ResItem? resItem = BuildResItem(item);

            return new BLLResponse { Content = resItem };
        }

        protected static ResItem? BuildResItem(Item? item)
        {
            ResItem? resItem = null;

            if (item != null)
            {
                resItem = new()
                {
                    Id = item.Id,
                    Category = new ResItemCategory()
                    {
                        Id = item.Category?.Id,
                        Name = item.Category?.Name,
                        Color = item.Category?.Color,
                        SubCategory = (item.SubCategory is not null) ? new ResItemSubCategory()
                        {
                            Id = item.SubCategory?.Id,
                            Name = item.SubCategory?.Name,
                            IconName = item.SubCategory?.IconName,
                        } : null,
                    },
                    Name = item.Name,
                    AcquisitionDate = item.AcquisitionDate,
                    AcquisitionType = (item.SubCategory is not null) ? new ResItemAcquisitionType()
                    {
                        Id = item.SubCategory?.Id,
                        Name = item.SubCategory?.Name,
                    } : null,
                    Comment = item.Comment,
                    Image1 = item.Image1,
                    Image2 = item.Image2,
                    CreatedAt = item.CreatedAt,
                    PurchaseStore = item.PurchaseStore,
                    PurchaseValue = item.PurchaseValue,
                    ResaleValue = item.ResaleValue,
                    Situation = new ResItemItemSituation() { Id = item.ItemSituation?.Id, Name = item.ItemSituation?.Name },
                    TechnicalDescription = item.TechnicalDescription,
                    UpdatedAt = item.UpdatedAt,
                    WithdrawalDate = item.WithdrawalDate,
                };
            }

            return resItem;
        }

        public BLLResponse UpdateItem(ReqItem reqItem, int uid, int id)
        {
            string? validateError = reqItem.Validate();
            if (!string.IsNullOrEmpty(validateError)) return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = validateError } };

            Item? oldItem = itemDAL.GetById(uid, id);

            if (oldItem == null)
                return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = "Invalid id" } };

            string? validateIndexes = ValidateIndexes(reqItem, uid);

            if (!string.IsNullOrEmpty(validateIndexes)) return new BLLResponse() { Content = null, Error = new ErrorMessage() { Error = validateIndexes } };

            Item item = new()
            {
                Id = id,
                AcquisitionDate = reqItem.AcquisitionDate,
                AcquisitionTypeId = reqItem.AcquisitionType,
                CategoryId = reqItem.Category.CategoryId,
                CreatedAt = oldItem.CreatedAt,
                ItemSituationId = reqItem.SituationId,
                Name = reqItem.Name,
                UpdatedAt = DateTime.Now,
                UserId = oldItem.UserId,
                Comment = reqItem.Comment,
                PurchaseStore = reqItem.PurchaseStore,
                PurchaseValue = reqItem.PurchaseValue,
                ResaleValue = reqItem.ResaleValue,
                SubCategoryId = reqItem.Category.SubCategoryId,
                TechnicalDescription = reqItem.TechnicalDescription,
                WithdrawalDate = reqItem.WithdrawalDate,
            };

            var respExec = itemDAL.Update(item);

            if (respExec == 1)
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

        public BLLResponse UpdateItemFileNames(int uid, int id, string? fileName1, string? fileName2)
        {
            var respExec = itemDAL.UpdateFileNames(uid, id, fileName1, fileName2);

            if (respExec == 1)
                return new BLLResponse { };
            else
                return new BLLResponse { Content = null, Error = new ErrorMessage() { Error = "Não foi possivel atualizar." } };
        }

        private string? ValidateIndexes(ReqItem reqItem, int uid)
        {
            if (itemSituationDAL.GetById(uid, reqItem.SituationId) == null)
                return "Situation with this id don't exist";

            if (categoryDAL.GetById(uid, reqItem.Category.CategoryId) == null)
                return "Category with this id don't exist";

            if ((reqItem.Category.SubCategoryId is not null) && (subCategoryDAL.GetById(uid, reqItem.Category.SubCategoryId.Value) == null))
                return "SubCategory with this id don't exist";

            if (acquisitionTypeDAL.GetById(uid, reqItem.AcquisitionType) == null)
                return "Acquisition Type with this id don't exist";

            return null;
        }
    }
}
