using InventoryModels;

namespace InventoryDAL.Interfaces
{
    public interface ISubCategoryDAL
    {
        SubCategory? GetById(int uid, int id);

        List<SubCategory>? GetByCategoryId(int uid, int categoryId);

        SubCategory? GetByCategoryIdAndName(int uid, int categoryId, string name);

        int Create(SubCategory subCategory);

        int Update(SubCategory subCategory);

        int Delete(SubCategory subCategory);
    }
}