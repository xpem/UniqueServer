using System.ComponentModel.DataAnnotations;

namespace BookshelfModels.Request
{
    public record ReqBookStatus : BaseModels.Request.ReqBaseModel
    {
        [Range(0, 4)]
        public required int Status { get; set; }

        [Range(0, 5)]
        public int? Score { get; set; } = null;

        [MaxLength(350)]
        public string? Comment { get; set; } = null;

    }
}
