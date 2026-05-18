using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialService.Model.Req
{
    public record AccountReq
    {
        public int? Id { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool Inactive { get; set; }
    }
}
