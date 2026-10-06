using System.Text.Json.Serialization;

namespace Shards.Model.DTO
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OreType
    {
        Bronze = 1,
        Silver = 2,
        Gold = 3,
        Ruby = 4,
        Sapphire = 5,
        Emerald = 6,
    }
}
