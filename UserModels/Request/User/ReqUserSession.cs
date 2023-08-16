using BaseModels;
using System.ComponentModel.DataAnnotations;

namespace UserModels.Request.User
{
    public record ReqUserSession : ReqUserEmail
    {
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(30, MinimumLength = 4)]
        public required string Password { get; init; }
    }
}
