using BaseModels.Request;
using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Model.Request.User
{
    public record ReqRefreshToken : ReqBaseModel
    {
        [Required(ErrorMessage = "RefreshToken is required")]
        public required string RefreshToken { get; init; }
    }
}
