using InventoryModels.DTOs;

namespace InventoryServices.Service
{
    public interface IItemHistoricService
    {
        Task<int> AddAsync(ItemHistoric itemHistoric);
        Task AddRangeAsync(List<ItemHistoric> itemHistorics);
        Task BuildAndCreateItemUpdateHistoricAsync(Item oldItem, Item newItem);
    }
}
