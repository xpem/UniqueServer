using System.ComponentModel.DataAnnotations;

namespace UserManagementModels.Request.User
{
    public record ReqUserSession : ReqUserEmail
    {
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(30, MinimumLength = 4)]
        public required string Password { get; init; }

        /// <summary>
        /// Identificador opcional da aplicação de origem do login (ex: "pixelpet", "bookshelf")
        /// </summary>
        [StringLength(50)]
        public string? Source { get; init; }
    }
}
