using InventoryModels.DTOs;

namespace InventoryRepos.Interfaces
{
    public interface ICategoryHistoricRepo
    {
        Task<int> AddAsync(CategoryHistoric categoryHistoric);
        Task AddRangeAsync(List<CategoryHistoric> categoryHistorics);
        Task<int> AddRangeItemListAsync(List<CategoryHistoricItem> categoryHistoricItems);
        Task<List<CategoryHistoric>> GetByCategoryIdAsync(int categoryId, int uid);
    }
}
