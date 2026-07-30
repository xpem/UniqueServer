using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryModels.Res.Item
{
    public class ResPaginatedListItems
    {
        public List<ResItem> Items { get; init; } = new List<ResItem>();
        public int TotalPages { get; init; }
        public int CurrentPage { get; init; }
    }
}
