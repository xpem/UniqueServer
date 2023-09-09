using System.ComponentModel.DataAnnotations;

namespace InventoryModels.Req
{
    public record ReqCategory : BaseModels.Request.ReqBaseModel
    {
        [StringLength(50)]
        public required string Name { get; init; }

        [StringLength(8)]
        public required string Color { get; init; }
    }
}
