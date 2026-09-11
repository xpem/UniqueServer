using BaseModels;
using InventoryModels.DTOs;
using InventoryModels.Res;
using InventoryRepos.Interfaces;

namespace InventoryServices.Service
{
    public class CategoryHistoricService(ICategoryHistoricRepo categoryHistoricRepo) : ICategoryHistoricService
    {
        public Task<int> AddAsync(CategoryHistoric categoryHistoric) => categoryHistoricRepo.AddAsync(categoryHistoric);

        public async Task<BaseResp> GetByCategoryIdAsync(int categoryId, int uid)
        {
            List<CategoryHistoric> historics = await categoryHistoricRepo.GetByCategoryIdAsync(categoryId, uid);

            List<ResCategoryHistoric> result = historics.Select(h => new ResCategoryHistoric
            {
                Id = h.Id,
                CreatedAt = h.CreatedAt,
                CategoryId = h.CategoryId,
                TypeId = h.CategoryHistoricTypeId,
                TypeName = h.CategoryHistoricType?.Name,
                Fields = h.CategoryHistoricItems.Select(f => new ResCategoryHistoricField
                {
                    Id = f.Id,
                    FieldId = f.CategoryHistoricItemFieldId,
                    FieldName = f.CategoryHistoricItemField?.Name,
                    UpdatedFrom = f.UpdatedFrom,
                    UpdatedTo = f.UpdatedTo
                }).ToList()
            }).ToList();

            return new BaseResp(result);
        }

        public async Task BuildAndCreateCategoryUpdateHistoricAsync(Category oldCategory, Category newCategory, int userId)
        {
            if (!HasChanges(oldCategory, newCategory)) return;

            CategoryHistoric categoryHistoric = new()
            {
                CategoryId = newCategory.Id,
                CategoryHistoricTypeId = 2,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await categoryHistoricRepo.AddAsync(categoryHistoric);

            List<CategoryHistoricItem> items = [];

            if (oldCategory.Name != newCategory.Name)
                items.Add(Field(1, oldCategory.Name, newCategory.Name, categoryHistoric.Id));

            if (oldCategory.Color != newCategory.Color)
                items.Add(Field(2, oldCategory.Color, newCategory.Color, categoryHistoric.Id));

            await categoryHistoricRepo.AddRangeItemListAsync(items);
        }

        private static bool HasChanges(Category oldCategory, Category newCategory) =>
            oldCategory.Name != newCategory.Name ||
            oldCategory.Color != newCategory.Color;

        private static CategoryHistoricItem Field(int fieldId, string from, string to, int historicId) =>
            new()
            {
                CategoryHistoricItemFieldId = fieldId,
                UpdatedFrom = from,
                UpdatedTo = to,
                CategoryHistoricId = historicId,
                CreatedAt = DateTime.UtcNow
            };
    }
}
