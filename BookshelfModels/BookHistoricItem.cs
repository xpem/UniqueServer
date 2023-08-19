using BaseModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookshelfModels
{
    public class BookHistoricItem: BaseModel
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
