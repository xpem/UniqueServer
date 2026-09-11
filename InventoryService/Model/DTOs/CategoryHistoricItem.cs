using BaseModels;
using System.ComponentModel.DataAnnotations;

namespace InventoryModels.DTOs
{
    public class CategoryHistoricItem : BaseModel
    {
        public required int CategoryHistoricItemFieldId { get; set; }

        public CategoryHistoricItemField? CategoryHistoricItemField { get; set; }

        [MaxLength(250)]
        public required string UpdatedFrom { get; set; }

        [MaxLength(250)]
        public required string UpdatedTo { get; set; }

        public required int CategoryHistoricId { get; set; }
    }
}
