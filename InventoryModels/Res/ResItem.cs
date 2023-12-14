using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryModels.Res
{
    public record ResItem
    {
        public int Id { get; init; }
        public string? Name { get; init; }

        public string? TechnicalDescription { get; init; }

        public DateOnly AcquisitionDate {  get; init; }

        public decimal? PurchaseValue { get; init; }

        public string? PurchaseStore { get; init; }

        public decimal? ResaleValue { get; init; }

    }
}
