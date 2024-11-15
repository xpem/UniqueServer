using InventoryModels;

namespace InventoryDAL.Interfaces
{
    public interface ISubCategoryRepo
    {
        SubCategory? GetById(int uid, int id);

        Task<List<SubCategory>?> GetByVersionAsync(int uid, int version, int page, int pageSize);

        List<SubCategory>? GetByCategoryId(int uid, int categoryId);

        SubCategory? GetByCategoryIdAndName(int uid, int categoryId, string name);

        Task<int> Create(SubCategory subCategory);

        Task<int> UpdateAsync(SubCategory subCategory);

        //int Delete(SubCategory subCategory);
    }
}