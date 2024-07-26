using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookshelfModels
{
    public class Book : BaseModels.BaseModel
    {
        [Index("IX_UpdatedAt_And_Uid", 1)]
        public required int UserId { get; set; }

        [MaxLength(100)]
        public required string Title { get; set; }

        [MaxLength(100)]
        public string? Subtitle { get; set; } = null;

        [MaxLength(150)]
        public required string Authors { get; set; }

        [MaxLength(3)]
        public int? Volume { get; set; } = null;

        [MaxLength(3)]
        public int? Pages { get; set; } = null;

        [MaxLength(4)]
        public int? Year { get; set; } = null;

        [MaxLength(2)]
        public required int Status { get; set; }

        [MaxLength(50)]
        public string? Genre { get; set; } = null;

        [MaxLength(100)]
        public string? Isbn { get; set; } = null;

        [MaxLength(2000)]
        public string? Cover { get; set; } = null;

        [MaxLength(200)]
        public string? GoogleId { get; set; } = null;

        [MaxLength(1)]
        public int? Score { get; set; } = null;

        [MaxLength(350)]
        public string? Comment { get; set; } = null;

        [Index("IX_UpdatedAt_And_Uid", 2)]
        public required DateTime UpdatedAt { get; set; }

        public bool Inactive { get; set; } = false;
    }
}