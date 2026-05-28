using FinancialService.Model.DTO;
using FinancialService.Model.Req;
using FinancialService.Model.Res;
using FinancialService.Repo;

namespace FinancialService.Service
{
    public interface IRecurringRuleService
    {
        Task<RecurringRuleRes> AddOrUpdateAsync(RecurringRuleReq req, int uid);
        Task<List<RecurringRuleRes>> GetByUpdatedAtAsync(int uid, DateTime updatedAt);
    }

    public class RecurringRuleService(IRecurringRuleRepo recurringRuleRepo) : IRecurringRuleService
    {
        public async Task<RecurringRuleRes> AddOrUpdateAsync(RecurringRuleReq req, int uid)
        {
            string? validationError = req.Validate();
            if (validationError != null)
                throw new ArgumentException(validationError);

            if (req.EndDate.HasValue && req.EndDate.Value < req.StartDate)
                throw new ArgumentException("End date must be on or after the start date.");

            RecurringRuleDTO dto = new()
            {
                RecurringRuleId = req.RecurringRuleId,
                Description = req.Description,
                Amount = req.Amount,
                Type = req.Type,
                CategoryId = req.CategoryId,
                AccountId = req.AccountId,
                Frequency = req.Frequency,
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                Inactive = req.Inactive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = uid,
            };

            await recurringRuleRepo.AddAsync(dto);

            return ToRes(dto);
        }

        public async Task<List<RecurringRuleRes>> GetByUpdatedAtAsync(int uid, DateTime updatedAt)
        {
            var rules = await recurringRuleRepo.GetByUpdatedAtAsync(uid, updatedAt);
            return rules.Select(ToRes).ToList();
        }

        private static RecurringRuleRes ToRes(RecurringRuleDTO dto) => new()
        {
            Id = dto.Id,
            RecurringRuleId = dto.RecurringRuleId,
            Description = dto.Description,
            Amount = dto.Amount,
            Type = (int)dto.Type,
            CategoryId = dto.CategoryId,
            AccountId = dto.AccountId,
            Frequency = (int)dto.Frequency,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Inactive = dto.Inactive,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
        };
    }
}
