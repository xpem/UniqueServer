using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace InventoryModels.DTOs
{
    [Index(nameof(ItemId), nameof(UserId), nameof(CreatedAt))]
    public class ItemHistoric
    {
        public int Id { get; set; }

        public required DateTime CreatedAt { get; set; }

        public required int UserId { get; set; }

        public required int ItemId { get; set; }

        public Item? Item { get; set; }

        public required int ItemHistoricTypeId { get; set; }

        public ItemHistoricType? ItemHistoricType { get; set; }

        [JsonIgnore]
        public List<ItemHistoricItem> ItemHistoricItems { get; set; } = [];
    }
}
