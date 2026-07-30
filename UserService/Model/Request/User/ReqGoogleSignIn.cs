using BaseModels.Request;
using System.ComponentModel.DataAnnotations;

namespace UserManagementModels.Request.User
{
    public record ReqGoogleSignIn : ReqBaseModel
    {
        [Display(Name = "IdToken")]
        [Required(ErrorMessage = "IdToken is required")]
        public required string IdToken { get; init; }
    }
}
