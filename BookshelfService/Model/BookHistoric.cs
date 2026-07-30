using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace BookshelfModels
{
    [Index(nameof(CreatedAt), nameof(UserId))]
    public class BookHistoric
    {
        public int Id { get; set; }

        public required DateTime CreatedAt { get; set; }

        public required int UserId { get; set; }

        public required int BookId { get; set; }

        public Book? Book { get; set; }

        public required int BookHistoricTypeId { get; set; }

        public BookHistoricType? BookHistoricType { get; set; }

        [JsonIgnore]
        public List<BookHistoricItem> BookHistoricItems { get; set; } = new List<BookHistoricItem>();
    }
}
