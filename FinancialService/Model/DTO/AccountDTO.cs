using System;
using System.Collections.Generic;
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

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        //public string Name { get; set; }

        //public decimal Balance { get; set; }

        public int? UserId { get; set; }
    }
}
