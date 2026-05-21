using FinancialService.Model.DTO;
using FinancialService.Model.Req;
using FinancialService.Repo;

namespace FinancialService.Service
{
    public interface ITransactionService
    {
        Task<string> AddAsync(TransactionReq req, int uid);
    }

    public class TransactionService(ITransactionRepo transactionRepo) : ITransactionService
    {
        public async Task<string> AddAsync(TransactionReq req, int uid)
        {
            string? validateError = req.Validate();

            if (validateError != null)
            {
                return validateError;
            }

            // Add your logic to process the transaction here

            TransactionDTO transactionDTO = new()
            {
                Id = req.Id,
                UpdatedAt = req.UpdatedAt,
                Inactive = req.Inactive,
                Description = req.Description,
                Date = req.Date,
                Amount = req.Amount,
                Repetition = req.Repetition,
                TotalInstallments = req.TotalInstallments,
                InstallmentId = req.InstallmentId,
                Installment = req.Installment,
                CategoryId = req.CategoryId,
                Type = req.Type,
                Note = req.Note,
                AccountId = req.AccountId,
                UserId = uid,
                CreatedAt = DateTime.Now,
            };

            await transactionRepo.AddAsync(transactionDTO);

            return "Transaction added successfully";
        }
    }
}
