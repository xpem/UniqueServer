using System.Text.Json.Serialization;

namespace Shards.Model.DTO
{
    /// <summary>
    /// Situação de um depósito. Sealed = defesa intacta (intocado); Open = já atacado, persiste até se esgotar;
    /// Depleted = estoque acabou; Expired = intocado de semana passada (na prática calculado na leitura, ver GameRules.EffectiveStatus).
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DepositStatus
    {
        Sealed = 0,
        Open = 1,
        Depleted = 2,
        Expired = 3,
    }
}
