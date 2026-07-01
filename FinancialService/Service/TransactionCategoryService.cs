using FinancialService.Model.DTO;
using FinancialService.Model.Req;
using FinancialService.Model.Res;
using FinancialService.Repo;

namespace FinancialService.Service
{
    public interface ITransactionCategoryService
    {
        Task<List<TransactionCategoryRes>> GetByUid(int uid, DateTime updatedAt);
        Task<TransactionCategoryUpsertRes> UpsertAsync(TransactionCategoryReq req, int uid);
    }

    public class TransactionCategoryService(ITransactionCategoryRepo transactionCategoryRepo) : ITransactionCategoryService
    {
        public async Task<List<TransactionCategoryRes>> GetByUid(int uid, DateTime updatedAt)
        {
            List<TransactionCategoryDTO> categories = await transactionCategoryRepo.GetByUid(uid, updatedAt);
            return [.. categories.Select(c => new TransactionCategoryRes
            {
                Id = c.Id,
                CategoryId = c.CategoryId,
                UpdatedAt = c.UpdatedAt,
                SystemDefault = c.SystemDefault,
                Name = c.Name,
                Inactive = c.Inactive,
                Color = c.Color,
                IsMainTransactionCategory = c.IsMainTransactionCategory,
                ParentTransactionCategoryId = c.ParentTransactionCategoryId
            })];
        }

        public async Task<TransactionCategoryUpsertRes> UpsertAsync(TransactionCategoryReq req, int uid)
        {
            if (req.CategoryId is not null && req.CategoryId != Guid.Empty)
            {
                var existing = await transactionCategoryRepo.FindByCategoryIdAsync(req.CategoryId.Value, uid);
                if (existing is not null)
                {
                    // Update mutable fields
                    existing.Name = req.Name;
                    existing.IsMainTransactionCategory = req.IsMainTransactionCategory;
                    existing.ParentTransactionCategoryId = req.ParentTransactionCategoryId;
                    existing.Inactive = req.Inactive;
                    existing.Color = req.Color;
                    existing.UpdatedAt = DateTime.UtcNow;
                    await transactionCategoryRepo.UpdateAsync(existing);
                    return new TransactionCategoryUpsertRes { Id = existing.Id };
                }
                else
                {
                    // Insert with provided CategoryId
                    var dto = new TransactionCategoryDTO
                    {
                        CategoryId = req.CategoryId,
                        Name = req.Name,
                        IsMainTransactionCategory = req.IsMainTransactionCategory,
                        ParentTransactionCategoryId = req.ParentTransactionCategoryId,
                        Inactive = req.Inactive,
                        Color = req.Color,
                        UserId = uid,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    var inserted = await transactionCategoryRepo.AddAsync(dto);
                    return new TransactionCategoryUpsertRes { Id = inserted.Id };
                }
            }
            else
            {
                // No CategoryId provided — generate one and insert
                var dto = new TransactionCategoryDTO
                {
                    CategoryId = Guid.NewGuid(),
                    Name = req.Name,
                    IsMainTransactionCategory = req.IsMainTransactionCategory,
                    ParentTransactionCategoryId = req.ParentTransactionCategoryId,
                    Inactive = req.Inactive,
                    Color = req.Color,
                    UserId = uid,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                var inserted = await transactionCategoryRepo.AddAsync(dto);
                return new TransactionCategoryUpsertRes { Id = inserted.Id };
            }
        }
    }
}
