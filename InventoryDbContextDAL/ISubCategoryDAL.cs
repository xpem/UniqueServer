using InventoryModels;

namespace InventoryDAL
{
    public interface ISubCategoryDAL
    {
        SubCategory? GetById(int uid, int id);

        List<SubCategory>? GetByCategoryId(int uid, int categoryId);

        SubCategory? GetByCategoryIdAndName(int uid, int categoryId, string name);

        Task<int> ExecuteCreateSubCategoryAsync(SubCategory subCategory);
    }
}