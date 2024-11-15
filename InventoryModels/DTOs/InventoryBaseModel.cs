using BaseModels;
using System.ComponentModel.DataAnnotations;

namespace InventoryModels.DTOs
{
    public class InventoryBaseModel : BaseModel
    {
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public bool SystemDefault { get; set; } = false;

        public int? UserId { get; set; }

        [MaxLength(50)]
        public required string Name { get; set; }

        public required int Version { get; set; }

        public bool Inactive { get; set; } = false;
    }
}
