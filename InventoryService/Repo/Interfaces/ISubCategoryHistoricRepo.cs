using InventoryModels.DTOs;

namespace InventoryRepos.Interfaces
{
    public interface ISubCategoryHistoricRepo
    {
        Task<int> AddAsync(SubCategoryHistoric subCategoryHistoric);
        Task AddRangeAsync(List<SubCategoryHistoric> subCategoryHistorics);
        Task<int> AddRangeItemListAsync(List<SubCategoryHistoricItem> subCategoryHistoricItems);
        Task<List<SubCategoryHistoric>> GetBySubCategoryIdAsync(int subCategoryId, int uid);
    }
}
