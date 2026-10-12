using System.Text.Json.Serialization;

namespace Shards.Model.DTO
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OreType
    {
        /// <summary>Quartzo: o minério comum da mina inicial e moeda básica do jogo.</summary>
        Quartz = 1,

        // 2 e 3 ficam sem uso (eram Silver e Gold): os valores das gemas não mudam para não alterar dados já gravados
        Ruby = 4,
        Sapphire = 5,
        Emerald = 6,
    }
}
