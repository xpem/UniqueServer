using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryModels.Res
{
    public record ResSubCategory
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? IconName { get; set; }

        public int CategoryId { get; set; }

        public bool SystemDefault { get; set; }
    }
}
