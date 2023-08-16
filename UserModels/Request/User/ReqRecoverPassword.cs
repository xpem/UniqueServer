using BaseModels.Request;
using System.ComponentModel.DataAnnotations;

namespace UserModels.Request.User
{
    public record ReqRecoverPassword : ReqBaseModel
    {
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(30, MinimumLength = 4)]
        public required string Password { get; init; }

        [Display(Name = "PasswordConfirmation")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password Confirmation is required")]
        [StringLength(30, MinimumLength = 4)]
        public required string PasswordConfirmation { get; init; }
    }

}
