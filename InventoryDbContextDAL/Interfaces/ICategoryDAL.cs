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
        List<Category>? GetWithSubCategories(int uid);
        int Update(Category category);
    }
}