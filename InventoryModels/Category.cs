using BaseModels;
using System.ComponentModel.DataAnnotations;

namespace InventoryModels
{
    public class Category : BaseModel
    {
        public required int UserId { get; set; }

        [MaxLength(50)]
        public required string Name { get; set; }

        [MaxLength(8)]
        public required string Color { get; set; }
               
        public bool SystemDefault { get; set; }

        public List<SubCategory>? SubCategories { get; set; }

        public required DateTime UpdatedAt { get; set; } = DateTime.Now;

    }
}
