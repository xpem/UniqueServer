using System.ComponentModel.DataAnnotations;

namespace UserModels
{
    public class UserHistoricType
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public required string Name { get; set; }
    }

    public enum UserHistoricTypeValues
    {
        SignIn = 1, PasswordChanged = 2
    }
}
