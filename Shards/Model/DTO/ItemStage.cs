using System.Text.Json.Serialization;

namespace Shards.Model.DTO
{
    /// <summary>
    /// Estágio de um item do inventário. Hoje tudo nasce e vive em Raw (bruto); Polished e Faceted são da futura
    /// Oficina de Lapidação. Todo custo e saldo do jogo atual olha só o estágio Raw.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ItemStage
    {
        /// <summary>Bruto. É o valor 0 de propósito: as linhas de inventário já gravadas viram Raw sem migrar dados.</summary>
        Raw = 0,
        Polished = 1,
        Faceted = 2,
    }
}
