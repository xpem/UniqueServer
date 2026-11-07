using System.Text.Json.Serialization;

namespace Shards.Model.DTO
{
    /// <summary>Tipo de um minério no catálogo (<c>Shards:Balance:Ores</c>).</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OreKind
    {
        /// <summary>A moeda do jogo (Quartz). Não tem elemento, não ataca e não se lapida.</summary>
        Currency = 0,

        /// <summary>Gema de mina comum (Rubi, Safira, Esmeralda): tem elemento e entra nas missões de coleção.</summary>
        Gem = 1,

        /// <summary>Minério exclusivo dos depósitos (Ametista). Não pode ser o tipo de produção de uma mina comum.</summary>
        Exclusive = 2,
    }
}
