using System.ComponentModel.DataAnnotations;

namespace BookshelfModels.Request
{
    public record ReqBook : BaseModels.Request.ReqBaseModel
    {
        [StringLength(2000)]
        [DataType(DataType.Url)]
        public string? Cover { get; init; }

        [StringLength(100)]
        public required string Title { get; init; }

        [StringLength(100)]
        public string? Subtitle { get; set; } = null;

        [StringLength(150)]
        public required string Authors { get; set; }

        [Range(0, 99)]
        public int? Volume { get; set; } = null;

        public int? Pages { get; set; } = null;

        [Range(0, 9999)]
        public int? Year { get; set; } = null;

        [Range(0, 4)]
        public required int Status { get; set; }

        [Range(0, 5)]
        public int? Score { get; set; } = null;

        [MaxLength(350)]
        public string? Comment { get; set; } = null;

        [MaxLength(50)]
        public string? Genre { get; set; } = null;

        [MaxLength(100)]
        public string? Isbn { get; set; } = null;

        [MaxLength(200)]
        public string? GoogleId { get; set; } = null;

        //public bool Inactive { get; set; } = false;


    }
}
