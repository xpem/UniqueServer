using InventoryModels.DTOs;

namespace InventoryRepos.Interfaces
{
    public interface IItemHistoricRepo
    {
        Task<int> AddAsync(ItemHistoric itemHistoric);
        Task AddRangeAsync(List<ItemHistoric> itemHistorics);
        Task<int> AddRangeItemListAsync(List<ItemHistoricItem> itemHistoricItems);
    }
}
