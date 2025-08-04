using BaseModels.Request;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryModels.Req
{
    public record ReqItem : ReqBaseModel
    {
        [StringLength(150)]
        public required string Name { get; init; }

        [StringLength(250)]
        public string? TechnicalDescription { get; init; }

        public required DateOnly AcquisitionDate { get; init; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? PurchaseValue { get; init; }

        [MaxLength(100)]
        public string? PurchaseStore { get; init; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? ResaleValue { get; init; }

        public required int SituationId { get; init; }

        public required ReqItemCategory Category { get; init; }

        [MaxLength(350)]
        public string? Comment { get; init; }

        public required int AcquisitionType { get; init; }

        public DateOnly? WithdrawalDate { get; init; }

    }

    public record ReqItemCategory
    {
        public required int CategoryId { get; init; }

        public int? SubCategoryId { get; init; }
    }


}
