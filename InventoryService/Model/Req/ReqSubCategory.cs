using System.ComponentModel.DataAnnotations;

namespace InventoryModels.Req
{
    public record ReqSubCategory : BaseModels.Request.ReqBaseModel
    {
        [StringLength(50)]
        public required string Name { get; init; }

        [StringLength(100)]
        public string? IconName { get; init; } = null;

        public required int CategoryId { get; init; }
    }
}
