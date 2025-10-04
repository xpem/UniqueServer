using BaseModels;
using InventoryBLL.Interfaces;
using InventoryModels.DTOs;
using InventoryModels.Req;
using InventoryModels.Res;
using InventoryModels.Res.Item;
using InventoryRepos.Interfaces;
using System.Threading.Tasks;

namespace InventoryBLL
{
    public class ItemService(IItemSituationRepo itemSituationRepo, ICategoryRepo categoryDAL,
        ISubCategoryRepo subCategoryDAL, IAcquisitionTypeRepo acquisitionTypeRepo,
        IItemRepo itemRepo) : IItemService
    {
        readonly int pageSize = 20;

        public async Task<BaseResponse> CreateItem(ReqItem reqItem, int uid)
        {
            try
            {
                string? validateError = reqItem.Validate();
                if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(null, validateError);

                //to do, não preciso validar os indices, serão validados pelas foreign keys no banco
                string? validateIndexes = await ValidateIndexes(reqItem, uid);
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

                try
                {
                    int resp = itemRepo.Create(item);

                    if (resp == 1)
                    {
                        Item? createdCompleteItem = itemRepo.GetById(uid, item.Id);

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
                catch (Exception ex) { throw ex; }
            }
            catch (Exception) { throw; }
        }

        //public BaseResponse ItemConfigs(int uid)
        //{
        //    try
        //    {
        //        var situations = itemSituationDAL.Get(uid);
        //        var categories = categoryDAL.Get(uid);
        //        var acquisitionTypes = acquisitionTypeDAL.Get(uid);
        //        ResItemConfigs itemConfigs = new()
        //        {
        //            ItemSituations = situations,
        //            Categories = categories,
        //            AcquisitionTypes = acquisitionTypes,
        //        };
        //        return new BaseResponse(itemConfigs);
        //    }
        //    catch (Exception) { throw; }
        //}

        public BaseResponse DeleteItem(int uid, int id, string filePath)
        {
            Item? item = itemRepo.GetById(uid, id);

            if (item == null)
                return new BaseResponse(null, "Invalid id");

            string? fileName1 = null, fileName2 = null;

            if (item.Image1 != null) fileName1 = item.Image1;
            if (item.Image2 != null) fileName2 = item.Image2;

            int respExec = itemRepo.Delete(item);

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
            Item? item = itemRepo.GetById(uid, id);

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

            int respExec = itemRepo.Update(item);

            if (respExec > 0)
            {
                Item? createdCompleteItem = itemRepo.GetById(uid, item.Id);

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

            List<Item>? items = await itemRepo.GetAsync(uid, page, pageSize);
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

        public async Task<BaseResponse> GetBySearch(int uid, int page, ReqSearchItem reqSearchItem)
        {
            if (page <= 0)
                return new BaseResponse(null, "Invalid page");

            List<Item>? items = await itemRepo.GetBySearchAsync(uid, page, pageSize, reqSearchItem);
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
            int totalItems = await itemRepo.GetTotalAsync(uid);

            double fractionalTotalPages = totalItems / (double)pageSize;

            if (!(fractionalTotalPages % 1 == 0)) fractionalTotalPages += 1;

            int totalPage = Convert.ToInt32(Math.Round(fractionalTotalPages, 0, MidpointRounding.ToZero));

            ResTotalItems resTotalItems = new() { TotalItems = totalItems, TotalPages = totalPage };

            return new BaseResponse(resTotalItems);
        }

        public async Task<BaseResponse> GetTotalItemsPagesBySearchAsync(int uid, ReqSearchItem reqSearchItem)
        {
            int totalItems = await itemRepo.GetTotalBySearchAsync(uid, reqSearchItem);

            double fractionalTotalPages = totalItems / (double)pageSize;

            if (!(fractionalTotalPages % 1 == 0)) fractionalTotalPages += 1;

            int totalPage = Convert.ToInt32(Math.Round(fractionalTotalPages, 0, MidpointRounding.ToZero));

            ResTotalItems resTotalItems = new() { TotalItems = totalItems, TotalPages = totalPage };

            return new BaseResponse(resTotalItems);
        }

        public BaseResponse GetById(int uid, int id)
        {
            Item? item = itemRepo.GetById(uid, id);

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

        public async Task<BaseResponse> UpdateItem(ReqItem reqItem, int uid, int id)
        {
            string? validateError = reqItem.Validate();
            if (!string.IsNullOrEmpty(validateError)) return new BaseResponse(null, validateError);

            Item? oldItem = itemRepo.GetById(uid, id);

            if (oldItem == null) return new BaseResponse(null, "Invalid id");

            string? validateIndexes = await ValidateIndexes(reqItem, uid);

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

            int respExec = itemRepo.Update(item);

            if (respExec == 1)
            {
                Item? createdCompleteItem = itemRepo.GetById(uid, item.Id);

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
            int respExec = itemRepo.UpdateFileNames(uid, id, fileName1, fileName2);

            if (respExec == 1)
                return new BaseResponse(new ResItemImages { Image1 = fileName1, Image2 = fileName2 });
            else
                return new BaseResponse(null, "Não foi possivel atualizar.");
        }

        public async Task<bool> CheckItemImageNameAsync(int uid, int id, string imageName) => await itemRepo.CheckItemImageNameAsync(uid, id, imageName);

        private async Task<string?> ValidateIndexes(ReqItem reqItem, int uid)
        {
            if ((await itemSituationRepo.GetById(uid, reqItem.SituationId)) == null)
                return "Situation with this id don't exist";

            if ((await categoryDAL.GetByIdAsync(uid, reqItem.Category.CategoryId)) == null)
                return "Category with this id don't exist";

            if ((reqItem.Category.SubCategoryId is not null) && ((await subCategoryDAL.GetById(uid, reqItem.Category.SubCategoryId.Value)) == null))
                return "SubCategory with this id don't exist";

            if ((await acquisitionTypeRepo.GetById(uid, reqItem.AcquisitionType)) == null)
                return "Acquisition Type with this id don't exist";

            return null;
        }

        /// <summary>
        /// to recover the lists needed on the upsert item screen
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        /// acquisition types, categories with subcategories, item situations, last 5 purchaseStores
        public async Task<BaseResponse> GetConfigs(int uid)
        {
            var itemSituationsDTO = await itemSituationRepo.Get(uid);

            List<ResItemSituation> resItemsSituations = ItemSituationService.BuildItemSituation(itemSituationsDTO);

            var acquisitionTypesDTO = await acquisitionTypeRepo.Get(uid);

            var resAcquisitionTypes = AcquisitionTypeService.BuildResAcquisitionType(acquisitionTypesDTO);

            var categoryWithSubCategoriesDTO = await categoryDAL.GetByIdWithSubCategoriesAsync(uid, null);

            var resCategoryWithSubCategories = CategoryService.BuildResCategoryWithSubCategories(categoryWithSubCategoriesDTO);

            var lastPurchaseStores = await itemRepo.GetLastPurchaseStores(uid, 5);

            ResItemConfigs itemConfigs = new()
            {
                ItemSituations = resItemsSituations,
                Categories = resCategoryWithSubCategories,
                AcquisitionTypes = resAcquisitionTypes,
                LastPurchaseStores = lastPurchaseStores,
            };

            return new BaseResponse(itemConfigs);
        }
    }
}
