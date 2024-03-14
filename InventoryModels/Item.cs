using BaseModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryModels
{
    public class Item : BaseModel
    {
        [Index("IX_Item_UserId")]
        public required int UserId { get; set; }

        [MaxLength(250)]
        public required string Name { get; set; }

        [MaxLength(350)]
        public string? TechnicalDescription { get; set; }

        public required DateOnly AcquisitionDate { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? PurchaseValue { get; set; }

        [MaxLength(100)]
        public string? PurchaseStore { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? ResaleValue { get; set; }

        public required int ItemSituationId { get; set; }

        public required int CategoryId { get; set; }

        public int? SubCategoryId { get; set; }

        public required int AcquisitionTypeId { get; set; }

        [MaxLength(350)]
        public string? Comment { get; set; }

        public string? Image1 { get; set; }

        public string? Image2 { get; set; }

        public ItemSituation? ItemSituation { get; set; }

        public Category? Category { get; set; }

        public SubCategory? SubCategory { get; set; }

        public AcquisitionType? AcquisitionType { get; set; }

        public required DateTime UpdatedAt { get; set; }

        public DateOnly? WithdrawalDate { get; set; }
    }
}
