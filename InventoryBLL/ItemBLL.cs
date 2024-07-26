using BaseModels;
using InventoryBLL.Interfaces;
using InventoryDAL.Interfaces;
using InventoryModels;
using InventoryModels.Req;
using InventoryModels.Res;
using InventoryModels.Res.Item;

namespace InventoryBLL
{
    public class ItemBLL(IItemSituationDAL itemSituationDAL, ICategoryDAL categoryDAL,
        ISubCategoryDAL subCategoryDAL, IAcquisitionTypeDAL acquisitionTypeDAL,
        IItemDAL itemDAL) : IItemBLL
    {
        readonly int pageSize = 50;

        public BaseResponse CreateItem(ReqItem reqItem, int uid)
        {
            try
            {
                string? validateError = reqItem.Validate();
                if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(null, validateError);

                string? validateIndexes = ValidateIndexes(reqItem, uid);
                if (!string.IsNullOrEmpty(validateIndexes)) return new BaseResponse(null, validateIndexes);

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

                int resp = itemDAL.Create(item);

                if (resp == 1)
                {
                    Item? createdCompleteItem = itemDAL.GetById(uid, item.Id);

                    if (createdCompleteItem != null)
                    {
                        ResItem? resItem = BuildResItem(createdCompleteItem);

                        return new BaseResponse(resItem);
                    }
                    else throw new Exception($"Não foi possivel recuperar o item de id: {item.Id}");
                }
                else
                    return new BaseResponse(null, "Não foi possivel adicionar.");

            }
            catch (Exception) { throw; }
        }

        public BaseResponse DeleteItem(int uid, int id, string filePath)
        {
            Item? item = itemDAL.GetById(uid, id);

            if (item == null)
                return new BaseResponse(null, "Invalid id");

            string? fileName1 = null, fileName2 = null;

            if (item.Image1 != null) fileName1 = item.Image1;
            if (item.Image2 != null) fileName2 = item.Image2;

            int respExec = itemDAL.Delete(item);

            if (respExec == 1)
            {
                if (fileName1 != null)
                    System.IO.File.Delete(Path.Combine(filePath, fileName1));

                if (fileName2 != null)
                    System.IO.File.Delete(Path.Combine(filePath, fileName2));

                return new BaseResponse(1);
            }
            else
                return new BaseResponse(null, "Não foi possivel excluir.");
        }

        public BaseResponse DeleteItemImage(int uid, int id, string fileName, string filePath)
        {
            Item? item = itemDAL.GetById(uid, id);

            if (item == null)
                return new BaseResponse(null, "Invalid id");

            if (item.Image1 != null && item.Image1 == fileName)
            {
                System.IO.File.Delete(Path.Combine(filePath, fileName));
                item.Image1 = null;
            }

            if (item.Image2 != null && item.Image2 == fileName)
            {
                System.IO.File.Delete(Path.Combine(filePath, fileName));
                item.Image2 = null;
            }

            int respExec = itemDAL.Update(item);

            if (respExec > 0)
            {
                Item? createdCompleteItem = itemDAL.GetById(uid, item.Id);

                if (createdCompleteItem != null)
                {
                    ResItem? resItem = BuildResItem(createdCompleteItem);

                    return new BaseResponse(resItem);
                }
                else throw new Exception($"Não foi possivel recuperar o item de id: {item.Id}");
            }
            else
                return new BaseResponse(null, "Não foi possivel atualizar o Item.");
        }

        public async Task<BaseResponse> GetAsync(int uid, int page)
        {
            if (page <= 0)
                return new BaseResponse(null, "Invalid page");

            List<Item>? items = await itemDAL.GetAsync(uid, page, pageSize);
            List<ResItem> resItems = [];

            if (items != null && items.Count > 0)
                foreach (Item item in items)
                {
                    ResItem? bildedResItem = BuildResItem(item);

                    if (bildedResItem != null)
                        resItems.Add(bildedResItem);
                }
            return new BaseResponse(resItems);
        }

        public async Task<BaseResponse> GetTotalItemsPagesAsync(int uid)
        {
            int totalItems = await itemDAL.GetTotalAsync(uid);

            double fractionalTotalPages = totalItems / (double)pageSize;

            if (!(fractionalTotalPages % 1 == 0)) fractionalTotalPages += 1;

            int totalPage = Convert.ToInt32(Math.Round(fractionalTotalPages, 0, MidpointRounding.ToZero));

            ResTotalItems resTotalItems = new() { TotalItems = totalItems, TotalPages = totalPage };

            return new BaseResponse(resTotalItems);
        }

        public BaseResponse GetById(int uid, int id)
        {
            Item? item = itemDAL.GetById(uid, id);

            if (item == null)
                return new BaseResponse(null, "Invalid id");

            ResItem? resItem = BuildResItem(item);

            return new BaseResponse(resItem);
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
                    AcquisitionType = (item.AcquisitionType is not null) ? new ResItemAcquisitionType()
                    {
                        Id = item.AcquisitionType?.Id,
                        Name = item.AcquisitionType?.Name,
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

        public BaseResponse UpdateItem(ReqItem reqItem, int uid, int id)
        {
            string? validateError = reqItem.Validate();
            if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(null, validateError);

            Item? oldItem = itemDAL.GetById(uid, id);

            if (oldItem == null) return new BaseResponse(null, "Invalid id");

            string? validateIndexes = ValidateIndexes(reqItem, uid);

            if (!string.IsNullOrEmpty(validateIndexes)) return new BaseResponse(null, validateIndexes);

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

            int respExec = itemDAL.Update(item);

            if (respExec == 1)
            {
                Item? createdCompleteItem = itemDAL.GetById(uid, item.Id);

                if (createdCompleteItem != null)
                {
                    ResItem? resItem = BuildResItem(createdCompleteItem);

                    return new BaseResponse(resItem);
                }
                else throw new Exception($"Não foi possivel recuperar o item de id: {item.Id}");
            }
            else
                return new BaseResponse(null, "Não foi possivel adicionar.");
        }

        public BaseResponse UpdateItemFileNames(int uid, int id, string? fileName1, string? fileName2)
        {
            int respExec = itemDAL.UpdateFileNames(uid, id, fileName1, fileName2);

            if (respExec == 1)
                return new BaseResponse(new ResItemImages { Image1 = fileName1, Image2 = fileName2 });
            else
                return new BaseResponse(null, "Não foi possivel atualizar.");
        }

        public async Task<bool> CheckItemImageNameAsync(int uid, int id, string imageName) => await itemDAL.CheckItemImageNameAsync(uid, id, imageName);

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
