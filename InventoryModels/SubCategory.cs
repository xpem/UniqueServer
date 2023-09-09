using BaseModels;
using System.ComponentModel.DataAnnotations;

namespace InventoryModels
{
    public class SubCategory : BaseModel
    {
        public required int UserId { get; set; }

        public required int CategoryId { get; set; }

        public Category? Category { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string IconName { get; set; }

        public bool SystemDefault { get; set; }

        public required DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}