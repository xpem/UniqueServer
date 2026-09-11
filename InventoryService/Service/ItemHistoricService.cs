using BaseModels;
using InventoryModels.DTOs;
using InventoryModels.Res.Item;
using InventoryRepos.Interfaces;

namespace InventoryServices.Service
{
    public class ItemHistoricService(IItemHistoricRepo itemHistoricRepo) : IItemHistoricService
    {
        public Task<int> AddAsync(ItemHistoric itemHistoric) => itemHistoricRepo.AddAsync(itemHistoric);

        public Task AddRangeAsync(List<ItemHistoric> itemHistorics) => itemHistoricRepo.AddRangeAsync(itemHistorics);

        public async Task<BaseResp> GetByItemIdAsync(int itemId, int uid)
        {
            List<ItemHistoric> historics = await itemHistoricRepo.GetByItemIdAsync(itemId, uid);

            List<ResItemHistoric> result = historics.Select(h => new ResItemHistoric
            {
                Id = h.Id,
                CreatedAt = h.CreatedAt,
                ItemId = h.ItemId,
                TypeId = h.ItemHistoricTypeId,
                TypeName = h.ItemHistoricType?.Name,
                Fields = h.ItemHistoricItems.Select(f => new ResItemHistoricField
                {
                    Id = f.Id,
                    FieldId = f.ItemHistoricItemFieldId,
                    FieldName = f.ItemHistoricItemField?.Name,
                    UpdatedFrom = f.UpdatedFrom,
                    UpdatedTo = f.UpdatedTo
                }).ToList()
            }).ToList();

            return new BaseResp(result);
        }

        public async Task BuildAndCreateItemUpdateHistoricAsync(Item oldItem, Item newItem)
        {
            if (!HasChanges(oldItem, newItem)) return;

            ItemHistoric itemHistoric = new()
            {
                ItemId = newItem.Id,
                ItemHistoricTypeId = 2,
                UserId = newItem.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await itemHistoricRepo.AddAsync(itemHistoric);

            List<ItemHistoricItem> items = [];

            if (oldItem.Name != newItem.Name)
                items.Add(Field(1, oldItem.Name, newItem.Name, itemHistoric.Id));

            if (oldItem.CategoryId != newItem.CategoryId)
                items.Add(Field(2,
                    oldItem.Category?.Name ?? oldItem.CategoryId.ToString(),
                    newItem.Category?.Name ?? newItem.CategoryId.ToString(),
                    itemHistoric.Id));

            if (oldItem.SubCategoryId != newItem.SubCategoryId)
                items.Add(Field(3,
                    oldItem.SubCategory?.Name ?? oldItem.SubCategoryId?.ToString() ?? "",
                    newItem.SubCategory?.Name ?? newItem.SubCategoryId?.ToString() ?? "",
                    itemHistoric.Id));

            if (oldItem.ItemSituationId != newItem.ItemSituationId)
                items.Add(Field(4,
                    oldItem.ItemSituation?.Name ?? oldItem.ItemSituationId.ToString(),
                    newItem.ItemSituation?.Name ?? newItem.ItemSituationId.ToString(),
                    itemHistoric.Id));

            if (oldItem.AcquisitionTypeId != newItem.AcquisitionTypeId)
                items.Add(Field(5,
                    oldItem.AcquisitionType?.Name ?? oldItem.AcquisitionTypeId.ToString(),
                    newItem.AcquisitionType?.Name ?? newItem.AcquisitionTypeId.ToString(),
                    itemHistoric.Id));

            if (oldItem.AcquisitionDate != newItem.AcquisitionDate)
                items.Add(Field(6, oldItem.AcquisitionDate.ToString(), newItem.AcquisitionDate.ToString(), itemHistoric.Id));

            if (oldItem.WithdrawalDate != newItem.WithdrawalDate)
                items.Add(Field(7,
                    oldItem.WithdrawalDate?.ToString() ?? "",
                    newItem.WithdrawalDate?.ToString() ?? "",
                    itemHistoric.Id));

            if (oldItem.PurchaseValue != newItem.PurchaseValue)
                items.Add(Field(8,
                    oldItem.PurchaseValue?.ToString() ?? "",
                    newItem.PurchaseValue?.ToString() ?? "",
                    itemHistoric.Id));

            if (oldItem.ResaleValue != newItem.ResaleValue)
                items.Add(Field(9,
                    oldItem.ResaleValue?.ToString() ?? "",
                    newItem.ResaleValue?.ToString() ?? "",
                    itemHistoric.Id));

            await itemHistoricRepo.AddRangeItemListAsync(items);
        }

        private static bool HasChanges(Item oldItem, Item newItem) =>
            oldItem.Name != newItem.Name ||
            oldItem.CategoryId != newItem.CategoryId ||
            oldItem.SubCategoryId != newItem.SubCategoryId ||
            oldItem.ItemSituationId != newItem.ItemSituationId ||
            oldItem.AcquisitionTypeId != newItem.AcquisitionTypeId ||
            oldItem.AcquisitionDate != newItem.AcquisitionDate ||
            oldItem.WithdrawalDate != newItem.WithdrawalDate ||
            oldItem.PurchaseValue != newItem.PurchaseValue ||
            oldItem.ResaleValue != newItem.ResaleValue;

        private static ItemHistoricItem Field(int fieldId, string from, string to, int historicId) =>
            new()
            {
                ItemHistoricItemFieldId = fieldId,
                UpdatedFrom = from,
                UpdatedTo = to,
                ItemHistoricId = historicId,
                CreatedAt = DateTime.UtcNow
            };
    }
}
