using System.Text.Json.Serialization;

namespace UserManagementModels
{
    public class UserHistoric : BaseModels.BaseModel
    {
        public required UserHistoricTypeValues UserHistoricTypeId { get; set; }

        public required int UserId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }
    }
}
