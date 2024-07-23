using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InventoryModels
{
    public class Category : InventoryBaseModel
    {
        [MaxLength(8)]
        public required string Color { get; set; }

        [JsonIgnore]
        public List<SubCategory>? SubCategories { get; set; }
    }
}
