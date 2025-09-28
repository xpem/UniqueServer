using BaseModels.Request;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryModels.Req
{
    public record ReqItem : ReqBaseModel
    {
        [StringLength(249)]
        [MaxLength(249, ErrorMessage = "O campo Nome deve ter no máximo 249 caracteres")]
        public required string Name { get; init; }

        [StringLength(250)]
        [MaxLength(349, ErrorMessage = "O campo Nome deve ter no máximo 349 caracteres")]
        public string? TechnicalDescription { get; init; }

        public required DateOnly AcquisitionDate { get; init; }

        [Range(0, 99999999.99, ErrorMessage = "O valor máximo permitido para PurchaseValue é 99.999.999,99.")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? PurchaseValue { get; init; }

        [MaxLength(99, ErrorMessage = "O campo Nome deve ter no máximo 99 caracteres")]
        public string? PurchaseStore { get; init; }

        [Range(0, 99999999.99, ErrorMessage = "O valor máximo permitido para ResaleValue é 99.999.999,99.")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? ResaleValue { get; init; }

        public required int SituationId { get; init; }

        public required ReqItemCategory Category { get; init; }

        [MaxLength(349, ErrorMessage = "O campo Nome deve ter no máximo 349 caracteres")]
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
