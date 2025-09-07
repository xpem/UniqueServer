using InventoryModels.DTOs;
using InventoryModels.Req;

namespace InventoryRepos.Interfaces
{
    public interface IItemRepo
    {
        int Create(Item item);

        int Delete(Item item);

        int Update(Item item);

        Item? GetById(int uid, int id);

        int UpdateFileNames(int uid, int id, string? fileName1, string? fileName2);

        Task<bool> CheckItemImageNameAsync(int uid, int id, string imageName);

        Task<int> GetTotalAsync(int uid, int[]? situationId);

        Task<List<Item>?> GetAsync(int uid, int page, int pageSize);

        Task<List<Item>?> GetBySearchAsync(int uid, int page, int pageSize, ReqSearchItem reqSearchItem);
    }
}