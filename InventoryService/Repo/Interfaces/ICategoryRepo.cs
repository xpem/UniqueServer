using InventoryModels.DTOs;

namespace InventoryRepos.Interfaces
{
    public interface ICategoryRepo
    {
        Task<int> CreateAsync(Category category);
        Task<int> DeleteAsync(Category category);
        Task<List<Category>?> GetAsync(int uid);
        Task<Category?> GetByIdAsync(int uid, int id);
        Task<List<Category>?> GetByIdWithSubCategoriesAsync(int uid, int? id = null);
        Task<Category?> GetByNameAsync(int uid, string name);
        Task<int> UpdateAsync(Category category);
    }
}