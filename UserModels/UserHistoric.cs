using System.Text.Json.Serialization;

namespace UserManagementModels
{
    public class UserHistoric : BaseModels.BaseModel
    {
        public required int UserHistoricTypeId { get; set; }

        [JsonIgnore]
        public UserHistoricType? UserHistoricType { get; set; }

        //[Index("IX_UserHistoric_UserId")]
        public required int UserId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }
    }
}
