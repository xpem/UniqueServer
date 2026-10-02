using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Model
{
    public class User() : BaseModels.BaseModel
    {
        [MaxLength(150)]
        public required string Name { get; set; }

        [MaxLength(250)]
        public required string Email { get; set; }

        [MaxLength(350)]
        public required string? Password { get; set; }

        public PasswordAlgo PasswordAlgo { get; set; } = PasswordAlgo.Legacy;

        public required bool IsGoogleAuth { get; set; } = false;

        [MaxLength(128)]
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiry { get; set; }
    }
}
