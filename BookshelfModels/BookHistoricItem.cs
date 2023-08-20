using BaseModels;
using System.ComponentModel.DataAnnotations;

namespace BookshelfModels
{
    public class BookHistoricItem : BaseModel
    {
        public required int BookHistoricItemFieldId { get; set; }

        public BookHistoricItemField? BookHistoricItemField { get; set; }

        [MaxLength(250)]
        public required string UpdatedFrom { get; set; }

        [MaxLength(250)]
        public required string UpdatedTo { get; set; }

        public required int BookHistoricId { get; set; }

        //[ForeignKey("BookHistoricId")]
        //public BookHistoric? BookHistoric { get; set; }
    }
}
