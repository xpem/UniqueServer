using BaseModels;
using InventoryModels.DTOs;
using InventoryModels.Res;
using InventoryRepos.Interfaces;

namespace InventoryServices.Service
{
    public class SubCategoryHistoricService(ISubCategoryHistoricRepo subCategoryHistoricRepo) : ISubCategoryHistoricService
    {
        public Task<int> AddAsync(SubCategoryHistoric subCategoryHistoric) => subCategoryHistoricRepo.AddAsync(subCategoryHistoric);

        public async Task<BaseResp> GetBySubCategoryIdAsync(int subCategoryId, int uid)
        {
            List<SubCategoryHistoric> historics = await subCategoryHistoricRepo.GetBySubCategoryIdAsync(subCategoryId, uid);

            List<ResSubCategoryHistoric> result = historics.Select(h => new ResSubCategoryHistoric
            {
                Id = h.Id,
                CreatedAt = h.CreatedAt,
                SubCategoryId = h.SubCategoryId,
                TypeId = h.SubCategoryHistoricTypeId,
                TypeName = h.SubCategoryHistoricType?.Name,
                Fields = h.SubCategoryHistoricItems.Select(f => new ResSubCategoryHistoricField
                {
                    Id = f.Id,
                    FieldId = f.SubCategoryHistoricItemFieldId,
                    FieldName = f.SubCategoryHistoricItemField?.Name,
                    UpdatedFrom = f.UpdatedFrom,
                    UpdatedTo = f.UpdatedTo
                }).ToList()
            }).ToList();

            return new BaseResp(result);
        }

        public async Task BuildAndCreateSubCategoryUpdateHistoricAsync(SubCategory oldSubCategory, SubCategory newSubCategory, int userId)
        {
            if (!HasChanges(oldSubCategory, newSubCategory)) return;

            SubCategoryHistoric subCategoryHistoric = new()
            {
                SubCategoryId = newSubCategory.Id,
                SubCategoryHistoricTypeId = 2,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await subCategoryHistoricRepo.AddAsync(subCategoryHistoric);

            List<SubCategoryHistoricItem> items = [];

            if (oldSubCategory.Name != newSubCategory.Name)
                items.Add(Field(1, oldSubCategory.Name, newSubCategory.Name, subCategoryHistoric.Id));

            if (oldSubCategory.IconName != newSubCategory.IconName)
                items.Add(Field(2,
                    oldSubCategory.IconName ?? "",
                    newSubCategory.IconName ?? "",
                    subCategoryHistoric.Id));

            await subCategoryHistoricRepo.AddRangeItemListAsync(items);
        }

        private static bool HasChanges(SubCategory oldSubCategory, SubCategory newSubCategory) =>
            oldSubCategory.Name != newSubCategory.Name ||
            oldSubCategory.IconName != newSubCategory.IconName;

        private static SubCategoryHistoricItem Field(int fieldId, string from, string to, int historicId) =>
            new()
            {
                SubCategoryHistoricItemFieldId = fieldId,
                UpdatedFrom = from,
                UpdatedTo = to,
                SubCategoryHistoricId = historicId,
                CreatedAt = DateTime.UtcNow
            };
    }
}
