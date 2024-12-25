using BaseModels.Request;
using System.ComponentModel.DataAnnotations;

namespace UserManagementModels.Request.User
{
    public record ReqUserDataExclusion : ReqBaseModel
    {
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$", ErrorMessage = "Invalid Email")]
        [StringLength(250, MinimumLength = 4)]
        public required string Email { get; init; }


        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(30, MinimumLength = 4)]
        public required string Password { get; init; }

        public string? UserDataBookshelf { get; init; }

        public string? UserAccount { get; init; }

    }
}
