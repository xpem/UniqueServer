using System.ComponentModel.DataAnnotations.Schema;

namespace BookshelfModels
{
    public class BookHistoric
    {
        public int Id { get; set; }

        [Index("IX_CreatedAt_And_Uid", 1)]
        public required DateTime CreatedAt { get; set; }

        [Index("IX_UpdatedAt_And_Uid", 2)]
        public required int UserId { get; set; }

        public required int BookId { get; set; }

        public Book? Book { get; set; }

        public required int BookHistoricTypeId { get; set; }

        public BookHistoricType? BookHistoricType { get; set; }

        public List<BookHistoricItem>? BookHistoricItems { get; set; }
    }
}
