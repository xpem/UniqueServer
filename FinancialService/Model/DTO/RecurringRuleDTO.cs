using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialService.Model.DTO
{
    [Table("RecurringRule")]
    public class RecurringRuleDTO
    {
        public int Id { get; set; }

        public Guid RecurringRuleId { get; set; }

        public required DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(250)]
        public string? Description { get; set; }

        public decimal Amount { get; set; }

        public TransactionType Type { get; set; }

        public int? CategoryId { get; set; }

        public int? AccountId { get; set; }

        public Frequency Frequency { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool Inactive { get; set; }

        public required int UserId { get; set; }
    }

    public enum Frequency
    {
        Monthly = 2,
    }
}
