using InventoryModels.DTOs;

namespace InventoryRepos.Interfaces
{
    public interface ICategoryRepo
    {
        int Create(Category category);
        int Delete(Category category);
        List<Category>? Get(int uid);
        Category? GetById(int uid, int id);
        Category? GetByName(int uid, string name);
        Task<List<Category>?> GetByIdWithSubCategories(int uid, int? id = null);
        int Update(Category category);
    }
}