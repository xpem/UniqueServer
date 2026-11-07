using System.Text.Json.Serialization;

namespace Shards.Model.DTO
{
    /// <summary>
    /// Elemento de um minério. Quem vence quem (e por quanto) está na matriz de efetividade <c>Combat.Effectiveness</c>.
    /// Neutral é só do quartzo (a moeda): todo outro minério tem um elemento. Um elemento novo entra junto do minério que o introduz.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Element
    {
        Neutral = 0,
        Fire = 1,
        Water = 2,
        Wind = 3,
    }
}
