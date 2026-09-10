using BaseModels;
using System.ComponentModel.DataAnnotations;

namespace InventoryModels.DTOs
{
    public class ItemHistoricItem : BaseModel
    {
        public required int ItemHistoricItemFieldId { get; set; }

        public ItemHistoricItemField? ItemHistoricItemField { get; set; }

        [MaxLength(250)]
        public required string UpdatedFrom { get; set; }

        [MaxLength(250)]
        public required string UpdatedTo { get; set; }

        public required int ItemHistoricId { get; set; }
    }
}
