using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryModels.Req
{
    public record ReqSearchItem(string? Name, int[]? Situations, ResultOrderBy? OrderBy);

    public enum ResultOrderBy
    {
        CreatedAt, Name, AcquisitionDate, UpdatedAt
    }
}
