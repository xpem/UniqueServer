using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace InventoryModels.DTOs
{
    [Index(nameof(CreatedAt), nameof(UserId))]
    public class SubCategoryHistoric
    {
        public int Id { get; set; }

        public required DateTime CreatedAt { get; set; }

        public required int UserId { get; set; }

        public required int SubCategoryId { get; set; }

        public SubCategory? SubCategory { get; set; }

        public required int SubCategoryHistoricTypeId { get; set; }

        public SubCategoryHistoricType? SubCategoryHistoricType { get; set; }

        [JsonIgnore]
        public List<SubCategoryHistoricItem> SubCategoryHistoricItems { get; set; } = [];
    }
}
