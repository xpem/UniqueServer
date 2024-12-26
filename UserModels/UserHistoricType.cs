using System.ComponentModel.DataAnnotations;

namespace UserManagementModels
{
    public class UserHistoricType
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public required string Name { get; set; }
    }

    public enum UserHistoricTypeValues
    {
        SignIn = 1, PasswordChanged = 2, DeleteBookshelfData = 3, DeleteUser = 4
    }
}
