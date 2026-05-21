using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialService.Model.DTO
{
    [Table("Transaction")]
    public class TransactionDTO
    {
        public int Id { get; set; }

        public required DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [StringLength(250)]
        public required string Description { get; set; }

        public DateTime Date { get; set; }

        public decimal Amount { get; set; }

        public Repetition Repetition { get; set; }

        public int? TotalInstallments { get; set; }

        public Guid? InstallmentId { get; set; }

        public int? Installment { get; set; }

        public int CategoryId { get; set; }

        public TransactionCategoryDTO? Category { get; set; }

        public TransactionType Type { get; set; }

        public string? Note { get; set; }

        public int AccountId { get; set; }

        public AccountDTO? Account { get; set; }

        public required int UserId { get; set; }

        public bool Inactive { get; set; }
    }

    public enum TransactionType
    {
        Income,
        Expense,
        Transfer,
        Adjustment,
    }

    public enum Repetition
    {
        None,
        Monthly,
        //Advanced
    }
}
