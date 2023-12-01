using InventoryModels;

namespace InventoryDAL
{
    public interface ISubCategoryDAL
    {
        SubCategory? GetById(int uid, int id);

        List<SubCategory>? GetByCategoryId(int uid, int categoryId);
    }
}