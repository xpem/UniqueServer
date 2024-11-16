using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UserModels
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
