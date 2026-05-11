using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UserManagementModels
{
    [Table("UserHistoric")]
    [Index(nameof(UserId), IsUnique = false)]
    public class UserHistoric : BaseModels.BaseModel
    {
        public required UserHistoricTypeValues UserHistoricTypeId { get; set; }

        public required int UserId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }
    }
}
