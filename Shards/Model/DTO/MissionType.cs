using System.Text.Json.Serialization;

namespace Shards.Model.DTO
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MissionType
    {
        FirstPickaxe = 1,
        Specialization = 2,
        OperationalExpansion = 3,
        FirstGem = 4,
        CartTraining = 5,
        CartUpgrade = 6,
        OpenWorkshop = 7,
        FirstPolish = 8,
        FirstFacet = 9,
        Diversification = 10,
        Collector = 11,
        PolishBatch = 12,
        FullSet = 13,
        FacetBatch = 14,
        ThirdMine = 15,
        FullCart = 16,
        DoubleLuck = 17,
    }
}
