using BaseModels;
using System.ComponentModel.DataAnnotations;

namespace InventoryModels
{
    public class InventoryBaseModel : BaseModel
    {
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public required bool SystemDefault { get; set; } = false;

        public int? UserId { get; set; }

        [MaxLength(50)]
        public required string Name { get; set; }
    }
}
