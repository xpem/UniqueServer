using System.Text.Json.Serialization;

namespace Shards.Model.DTO
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SkillType
    {
        DoubleDrop = 1,
        MineSlots = 2,
        CartCapacity = 3,

        /// <summary>Oficina de Lapidação: cada nível libera um estágio (1 = polido, 2 = facetado).</summary>
        Workshop = 4,
    }
}
