using FinancialService.Model.DTO;
using System.ComponentModel.DataAnnotations;

namespace FinancialService.Model.Req
{
    public record TransactionReq : BaseReq
    {
        public int? Id { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool Inactive { get; set; }

        [StringLength(250)]
        public required string Description { get; set; }

        public DateTime Date { get; set; }

        public decimal Amount { get; set; }

        public Repetition Repetition { get; set; }

        public int? TotalInstallments { get; set; }

        public Guid? InstallmentId { get; set; }

        public int? Installment { get; set; }

        public int? CategoryId { get; set; }

        public TransactionType Type { get; set; }

        public string? Note { get; set; }

        public int AccountId { get; set; }

        /// <summary>
        /// Stable identifier of the recurring rule that originated this occurrence.
        /// </summary>
        public Guid? RecurringRuleId { get; set; }

        /// <summary>
        /// True when this recurring occurrence has been individually edited.
        /// </summary>
        public bool IsCustomized { get; set; }
    }
}
