using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialService.Model.DTO
{
    [Table("Account")]
    public class AccountDTO
    {
        public int Id { get; set; }

        public required DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public required string Name { get; set; }

        public AccountType Type { get; set; }

        public decimal CurrentBalance { get; set; }

        public bool IncludeInGeneralBalance { get; set; } = true;

        public bool Inactive { get; set; }

        public required int UserId { get; set; }
    }
}
