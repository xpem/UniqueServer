using FinancialService.Model.Dto;
using FinancialService.Model.Res;
using FinancialService.Repo;

namespace FinancialService.Service
{
    public interface ITransactionCategoryService
    {
        Task<List<TransactionCategoryRes>> GetByUid(int uid, DateTime updatedAt);
    }

    public class TransactionCategoryService(ITransactionCategoryRepo transactionCategoryRepo) : ITransactionCategoryService
    {
        public async Task<List<TransactionCategoryRes>> GetByUid(int uid, DateTime updatedAt)
        {
            List<TransactionCategoryDTO> categories = await transactionCategoryRepo.GetByUid(uid, updatedAt);
            return [.. categories.Select(c => new TransactionCategoryRes
            {
                Id = c.Id,
                UpdatedAt = c.UpdatedAt,
                SystemDefault = c.SystemDefault,
                Name = c.Name,
                Inactive = c.Inactive,
                Color = c.Color,
                IsMainTransactionCategory = c.IsMainTransactionCategory,
                ParentTransactionCategoryId = c.ParentTransactionCategoryId
            })];
        }
    }
}
