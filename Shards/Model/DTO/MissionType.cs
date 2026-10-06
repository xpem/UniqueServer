using System.Text.Json.Serialization;

namespace Shards.Model.DTO
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MissionType
    {
        FirstPickaxe = 1,
        Specialization = 2,
        OperationalExpansion = 3,
    }
}
