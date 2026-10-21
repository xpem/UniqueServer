using System.Text.Json.Serialization;

namespace Shards.Model.DTO
{
    /// <summary>Tarefas básicas diárias. Os valores são gravados no banco: não renumerar.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DailyMissionType
    {
        /// <summary>Entre no jogo: concluída na criação das diárias do dia.</summary>
        Login = 1,
        Energy = 2,
        Cart = 3,
        Quartz = 4,
        Gems = 5,
        Polish = 6,
        Facet = 7,
    }
}
