using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace InventoryModels.DTOs
{
    [Index(nameof(CategoryId), nameof(UserId), nameof(CreatedAt))]
    public class CategoryHistoric
    {
        public int Id { get; set; }

        public required DateTime CreatedAt { get; set; }

        public required int UserId { get; set; }

        public required int CategoryId { get; set; }

        public Category? Category { get; set; }

        public required int CategoryHistoricTypeId { get; set; }

        public CategoryHistoricType? CategoryHistoricType { get; set; }

        [JsonIgnore]
        public List<CategoryHistoricItem> CategoryHistoricItems { get; set; } = [];
    }
}
