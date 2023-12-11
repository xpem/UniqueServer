using BaseModels;
using System.ComponentModel.DataAnnotations;

namespace InventoryModels
{
    public class Category : InventoryBaseModel
    {
        [MaxLength(8)]
        public required string Color { get; set; }

        public List<SubCategory>? SubCategories { get; set; }


    }
}
