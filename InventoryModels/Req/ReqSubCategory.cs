using System.ComponentModel.DataAnnotations;

namespace InventoryModels.Req
{
    public record ReqSubCategory : BaseModels.Request.ReqBaseModel
    {
        [StringLength(50)]
        public required string Name { get; init; }

        [StringLength(100)]
        public required string IconName { get; init; }

        public required int CategoryId { get; init; }
    }
}
