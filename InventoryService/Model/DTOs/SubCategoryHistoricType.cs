using System.ComponentModel.DataAnnotations;

namespace InventoryModels.DTOs
{
    public class SubCategoryHistoricType
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public required string Name { get; set; }
    }
}
