namespace BookshelfModels
{
    public class BookHistoric : BaseModels.BaseModel
    {
        public required int BookId { get; set; }

        public Book? Book { get; set; }

        public required int BookHistoricTypeId { get; set; }

        public BookHistoricType? BookHistoricType { get; set; }
    }
}
