using System.ComponentModel.DataAnnotations;

namespace InventoryModels.Req
{
    public record ReqItemBulk : ReqItem
    {
        [Range(2, 99, ErrorMessage = "Quantidade deve ser entre 2 e 99.")]
        public required int Quantity { get; init; }
    }
}
