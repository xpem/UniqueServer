using System.ComponentModel.DataAnnotations;

namespace InventoryModels.DTOs
{
    public class SubCategory : InventoryBaseModel
    {
        public required int CategoryId { get; set; }

        public Category? Category { get; set; }

        [MaxLength(100)]
        public string? IconName { get; set; }
    }
}