using System.ComponentModel.DataAnnotations;

namespace UserManagementModels
{
    public class User : BaseModels.BaseModel
    {
        [MaxLength(150)]
        public required string Name { get; set; }

        [MaxLength(250)]
        public required string Email { get; set; }

        [MaxLength(350)]
        public required string Password { get; set; }
    }
}
