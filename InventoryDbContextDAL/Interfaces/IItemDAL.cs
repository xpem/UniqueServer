using InventoryModels;

namespace InventoryDAL.Interfaces
{
    public interface IItemDAL
    {
        int Create(Item item);

        int Delete(Item item);

        int Update(Item item);

        Item? GetById(int uid, int id);

        int UpdateFileNames(int uid, int id, string? fileName1, string? fileName2);

        Task<int> GetTotalAsync(int uid);

        Task<List<Item>?> GetAsync(int uid, int page, int pageSize);
    }
}