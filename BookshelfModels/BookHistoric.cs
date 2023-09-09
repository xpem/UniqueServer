namespace BookshelfModels
{
    public class BookHistoric : BaseModels.BaseModel
    {
        public required int UserId { get; set; }

        public required int BookId { get; set; }

        public Book? Book { get; set; }

        public required int BookHistoricTypeId { get; set; }

        public BookHistoricType? BookHistoricType { get; set; }

        public List<BookHistoricItem>? BookHistoricItems { get; set; }
    }
}
