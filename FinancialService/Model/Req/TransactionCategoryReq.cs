using System.ComponentModel.DataAnnotations;

namespace FinancialService.Model.Req
{
    public record TransactionCategoryReq : BaseReq
    {
        /// <summary>
        /// Stable cross-device identifier. Nullable for backward compatibility.
        /// </summary>
        public Guid? CategoryId { get; set; }

        [StringLength(50)]
        public required string Name { get; set; }

        public bool IsMainTransactionCategory { get; set; }

        public int? ParentTransactionCategoryId { get; set; }

        public bool Inactive { get; set; }

        [StringLength(8)]
        public string? Color { get; set; }

        /// <summary>
        /// Category type classification: 0 = Income, 1 = Expense, 2 = Both.
        /// Nullable for backward compatibility with older clients that do not send this field.
        /// </summary>
        public int? Type { get; set; }
    }
}
