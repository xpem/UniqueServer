using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialService.Model.Res
{
    public record TransactionCategoryRes
    {
        public int Id { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public bool SystemDefault { get; set; } = false;

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
