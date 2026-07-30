using System.ComponentModel.DataAnnotations;

namespace InventoryModels.Req
{
    public record ReqCategory : BaseModels.Request.ReqBaseModel
    {
        [StringLength(50)]
        public required string Name { get; init; }

        [StringLength(8)]
        public string Color { get; init; } = "3C3C3C";
    }
}
