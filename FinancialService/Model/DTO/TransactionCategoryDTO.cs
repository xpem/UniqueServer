using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialService.Model.Dto
{
    [Table("TransactionCategory")]
    public class TransactionCategoryDTO
    {
        public int Id { get; set; }

        public required DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public bool SystemDefault { get; set; } = false;

        public int? UserId { get; set; }

        [MaxLength(50)]
        public required string Name { get; set; }

        public bool Inactive { get; set; } = false;

        [MaxLength(8)]
        public string? Color { get; set; }
       
        public bool IsMainTransactionCategory { get; set; }

        //se nao for uma categoria principal, deve ter o id da categoria principal
        public int? ParentTransactionCategoryId { get; set; }
    }
}
