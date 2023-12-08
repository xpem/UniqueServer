using BaseModels;
using System.ComponentModel.DataAnnotations;

namespace InventoryModels
{
    public class SubCategory : BaseModel
    {
        public int? UserId { get; set; }

        public required int CategoryId { get; set; }

        public Category? Category { get; set; }

        [MaxLength(50)]
        public required string Name { get; set; }

        [MaxLength(100)]
        public string? IconName { get; set; }

        public required bool SystemDefault { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}