using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialService.Model.Req
{
    public record AdjustmentTransactionReq
    {
        public AccountReq AccountReq { get; set; }

    }
}
