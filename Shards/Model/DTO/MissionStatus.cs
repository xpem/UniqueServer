using System.Text.Json.Serialization;

namespace Shards.Model.DTO
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MissionStatus
    {
        Active = 0,
        Completed = 1,
        Claimed = 2,
    }
}
