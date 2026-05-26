using FinancialService.Model.DTO;
using System.ComponentModel.DataAnnotations;

namespace FinancialService.Model.Req
{
    public record RecurringRuleReq : BaseReq
    {
        public Guid RecurringRuleId { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        public TransactionType Type { get; set; }

        public int? CategoryId { get; set; }

        public int? AccountId { get; set; }

        public Frequency Frequency { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool Inactive { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
