using System.ComponentModel.DataAnnotations;

namespace InventoryModels.Req
{
    public record ReqItemBulk : ReqItem
    {
        [Range(2, 10, ErrorMessage = "Quantidade deve ser entre 2 e 10.")]
        public required int Quantity { get; init; }
    }
}
