using FinancialService.Model.Res;
using FinancialService.Model.Req;
using FinancialService.Repo;
using FinancialService.Model.DTO;

namespace FinancialService.Service
{
    public interface IAccountService
    {
        Task<AdjustAccountBalanceReq> AdjustAccountBalanceAsync(AdjustAccountBalanceReq req, int uid);
        Task<AccountRes?> GetAsync(int uid, DateTime updatedAt);
    }

    public class AccountService(IAccountRepo accountRepo, ITransactionRepo transactionRepo) : IAccountService
    {
        public async Task<AccountRes?> GetAsync(int uid, DateTime updatedAt)
        {
            var account = await accountRepo.GetAsync(uid);

            if (account is null || account.UpdatedAt <= updatedAt) return null;

            return new AccountRes
            {
                Id = account.Id,
                UpdatedAt = account.UpdatedAt,
                Inactive = false,
            };
        }
        public async Task<AdjustAccountBalanceReq> AdjustAccountBalanceAsync(AdjustAccountBalanceReq req, int uid)
        {
            var existingAccount = await accountRepo.GetAsync(uid);

            if (existingAccount == null)
            {
                existingAccount = new AccountDTO
                {
                    UserId = uid,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                await accountRepo.Add(existingAccount);
            }

            TransactionDTO transactionDTO = new()
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Inactive = req.Transaction.Inactive,
                Description = req.Transaction.Description,
                Date = req.Transaction.Date,
                Amount = req.Transaction.Amount,
                Repetition = req.Transaction.Repetition,
                TotalInstallments = req.Transaction.TotalInstallments,
                InstallmentId = req.Transaction.InstallmentId,
                Installment = req.Transaction.Installment,
                CategoryId = req.Transaction.CategoryId,
                Type = req.Transaction.Type,
                Note = req.Transaction.Note,
                AccountId = existingAccount.Id,
                UserId = uid,
            };

            await transactionRepo.AddAsync(transactionDTO);

            req.Id = existingAccount.Id;
            req.Transaction.Id = transactionDTO.Id;

            return req;
        }
    }
}
