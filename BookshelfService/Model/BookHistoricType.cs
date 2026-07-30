using System.ComponentModel.DataAnnotations;

namespace BookshelfModels
{
    public class BookHistoricType
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public required string Name { get; set; }
    }
}
