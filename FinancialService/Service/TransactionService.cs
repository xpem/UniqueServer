using FinancialService.Model.DTO;
using FinancialService.Model.Req;
using FinancialService.Model.Res;
using FinancialService.Repo;

namespace FinancialService.Service
{
    public interface ITransactionService
    {
        Task<TransactionDTO> AddAsync(TransactionReq req, int uid);
        Task<List<TransactionRes>> GetByUpdatedAtAsync(int uid, DateTime updatedAt);
    }

    public class TransactionService(ITransactionRepo transactionRepo) : ITransactionService
    {
        public async Task<TransactionDTO> AddAsync(TransactionReq req, int uid)
        {
            string? validateError = req.Validate();

            if (validateError != null)
                throw new ArgumentException(validateError);

            TransactionDTO transactionDTO = new()
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Inactive = req.Inactive,
                Description = req.Description,
                Date = req.Date,
                Amount = req.Amount,
                Repetition = req.Repetition,
                TotalInstallments = req.TotalInstallments,
                InstallmentId = req.InstallmentId,
                Installment = req.Installment,
                CategoryId = req.CategoryId,                Type = req.Type,
                Note = req.Note,
                AccountId = req.AccountId,
                UserId = uid,
            };

            await transactionRepo.AddAsync(transactionDTO);

            return transactionDTO;
        }

        public async Task<List<TransactionRes>> GetByUpdatedAtAsync(int uid, DateTime updatedAt)
        {
            var transactions = await transactionRepo.GetByUpdatedAtAsync(uid, updatedAt);

            return transactions.Select(t => new TransactionRes
            {
                Id = t.Id,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                Inactive = t.Inactive,
                Description = t.Description,
                Date = t.Date,
                Amount = t.Amount,
                Repetition = (int)t.Repetition,
                TotalInstallments = t.TotalInstallments,
                InstallmentId = t.InstallmentId,
                Installment = t.Installment,
                CategoryId = t.CategoryId,
                Type = (int)t.Type,
                Note = t.Note,
                AccountId = t.AccountId,
            }).ToList();
        }
    }
}
