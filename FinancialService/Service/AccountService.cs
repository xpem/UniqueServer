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
        Task<AccountRes> CreateAsync(AccountReq req, int uid);
        Task<AccountRes> UpdateAsync(int id, AccountReq req, int uid);
        Task<List<AccountRes>> GetUpdatedAfterAsync(int uid, DateTime updatedAt);
        Task RecalculateBalanceAsync(int accountId, int uid);
    }

    public class AccountService(IAccountRepo accountRepo, ITransactionRepo transactionRepo) : IAccountService
    {
        public async Task<AccountRes?> GetAsync(int uid, DateTime updatedAt)
        {
            var accounts = await accountRepo.GetUpdatedAfterAsync(uid, updatedAt);

            if (accounts.Count == 0) return null;

            var account = accounts[0];
            return new AccountRes
            {
                Id = account.Id,
                Name = account.Name,
                Type = account.Type,
                CurrentBalance = account.CurrentBalance,
                IncludeInGeneralBalance = account.IncludeInGeneralBalance,
                Inactive = account.Inactive,
                UpdatedAt = account.UpdatedAt,
                AccountId = account.AccountId,
            };
        }
        public async Task<AdjustAccountBalanceReq> AdjustAccountBalanceAsync(AdjustAccountBalanceReq req, int uid)
        {
            var accounts = await accountRepo.GetAllAsync(uid);
            var existingAccount = accounts.FirstOrDefault();

            if (existingAccount == null)
            {
                existingAccount = new AccountDTO
                {
                    Name = "Conta Principal",
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
        public async Task<AccountRes> CreateAsync(AccountReq req, int uid)
        {
            string? validateError = req.Validate();
            if (validateError != null)
                throw new ArgumentException(validateError);

            if (!Enum.IsDefined(typeof(AccountType), req.Type))
                throw new ArgumentException("Invalid account type.");

            // Determine if we have a valid AccountId to upsert by
            Guid? accountId = req.AccountId is not null && req.AccountId.Value != Guid.Empty
                ? req.AccountId.Value
                : null;

            if (accountId is not null)
            {
                // Upsert path: look for existing record by AccountId + UserId
                var existing = await accountRepo.FindByAccountIdAsync(accountId.Value, uid);
                if (existing is not null)
                {
                    // Update mutable fields
                    existing.Name = req.Name;
                    existing.Type = req.Type;
                    existing.IncludeInGeneralBalance = req.IncludeInGeneralBalance;
                    existing.Inactive = req.Inactive;
                    existing.UpdatedAt = DateTime.UtcNow;

                    await accountRepo.Update(existing);
                    return MapToRes(existing);
                }
            }

            // Insert path: either no AccountId provided, or no existing record found
            AccountDTO account = new()
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Name = req.Name,
                Type = req.Type,
                IncludeInGeneralBalance = req.IncludeInGeneralBalance,
                Inactive = req.Inactive,
                CurrentBalance = 0,
                UserId = uid,
                AccountId = accountId ?? Guid.NewGuid(),
            };

            await accountRepo.Add(account);
            return MapToRes(account);
        }
        public async Task<AccountRes> UpdateAsync(int id, AccountReq req, int uid)
        {
            var account = await accountRepo.GetByIdAsync(id, uid)
                ?? throw new KeyNotFoundException($"Account {id} not found.");

            account.Name = req.Name;
            account.Type = req.Type;
            account.IncludeInGeneralBalance = req.IncludeInGeneralBalance;
            account.Inactive = req.Inactive;
            account.UpdatedAt = DateTime.UtcNow;

            await accountRepo.Update(account);

            return MapToRes(account);
        }
        public async Task<List<AccountRes>> GetUpdatedAfterAsync(int uid, DateTime updatedAt)
        {
            var accounts = await accountRepo.GetUpdatedAfterAsync(uid, updatedAt);
            return accounts.Select(MapToRes).ToList();
        }
        public async Task RecalculateBalanceAsync(int accountId, int uid)
        {
            var account = await accountRepo.GetByIdAsync(accountId, uid)
                ?? throw new KeyNotFoundException($"Account {accountId} not found.");

            decimal sum = await transactionRepo.GetSumByAccountIdAsync(accountId);
            account.CurrentBalance = sum;
            account.UpdatedAt = DateTime.UtcNow;

            await accountRepo.Update(account);
        }
        private static AccountRes MapToRes(AccountDTO account)
        {
            return new AccountRes
            {
                Id = account.Id,
                Name = account.Name,
                Type = account.Type,
                CurrentBalance = account.CurrentBalance,
                IncludeInGeneralBalance = account.IncludeInGeneralBalance,
                Inactive = account.Inactive,
                UpdatedAt = account.UpdatedAt,
                AccountId = account.AccountId,
            };
        }
    }
}
