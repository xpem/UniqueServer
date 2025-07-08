using InventoryModels.DTOs;

namespace InventoryRepos.Interfaces
{
    public interface ICategoryDAL
    {
        int Create(Category category);
        int Delete(Category category);
        List<Category>? Get(int uid);
        Category? GetById(int uid, int id);
        Category? GetByName(int uid, string name);
        Task<List<Category>?> GetWithSubCategories(int uid, int? id = null);
        int Update(Category category);
    }
}