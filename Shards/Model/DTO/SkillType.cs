using System.Text.Json.Serialization;

namespace Shards.Model.DTO
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SkillType
    {
        DoubleDrop = 1,
        MineSlots = 2,
        CartCapacity = 3,
    }
}
