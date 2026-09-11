using BaseModels;
using System.ComponentModel.DataAnnotations;

namespace InventoryModels.DTOs
{
    public class SubCategoryHistoricItem : BaseModel
    {
        public required int SubCategoryHistoricItemFieldId { get; set; }

        public SubCategoryHistoricItemField? SubCategoryHistoricItemField { get; set; }

        [MaxLength(250)]
        public required string UpdatedFrom { get; set; }

        [MaxLength(250)]
        public required string UpdatedTo { get; set; }

        public required int SubCategoryHistoricId { get; set; }
    }
}
