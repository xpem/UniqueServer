using InventoryModels.DTOs;

namespace InventoryRepos.Interfaces
{
    public interface ISubCategoryRepo
    {
        Task<SubCategory?> GetById(int uid, int id);

        //Task<SubCategory?> GetByIdWithCategory(int uid, int id);

        Task<List<SubCategory>> GetByAfterUpdatedAtAsync(int uid, DateTime updatedAt, int page, int pageSize);

        Task<List<SubCategory>?> GetByCategoryIdAsync(int uid, int categoryId);

        Task<SubCategory?> GetByCategoryIdAndNameAsync(int uid, int categoryId, string name);

        Task<int> CreateAsync(SubCategory subCategory);

        Task<int> UpdateAsync(SubCategory subCategory);

        //int Delete(SubCategory subCategory);
    }
}