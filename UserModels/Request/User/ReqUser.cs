using System.ComponentModel.DataAnnotations;

namespace UserManagementModels.Request.User
{
    public record ReqUser : ReqUserSession
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(150, MinimumLength = 4)]
        public required string Name { get; init; }
    }
}
