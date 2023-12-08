using InventoryModels;

namespace InventoryDAL
{
    public interface ICategoryDAL
    {
        int Create(Category category);
        int Delete(int uid, int id);
        List<Category>? Get(int uid);
        Category? GetById(int uid, int id);
        Category? GetByName(int uid, string name);
        List<Category>? GetWithSubCategories(int uid);
        int Update(Category category);
    }
}