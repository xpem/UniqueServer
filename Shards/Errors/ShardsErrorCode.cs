namespace Shards.Errors
{
    /// <summary>Códigos de erro de regra de negócio do Shards, devolvidos ao cliente em { code, message }.</summary>
    public enum ShardsErrorCode
    {
        PlayerNotFound = 1,
        MineNotFound = 2,
        NotEnoughEnergy = 3,
        CartEmpty = 4,
        NoSkillPoints = 5,
        SkillMaxLevel = 6,
        InvalidSkill = 7,
        NotEnoughResources = 8,
        NoMoreMines = 9,
        MissionNotCompleted = 10,
        MissionNotFound = 11,
        InvalidTimes = 12,
        ConcurrencyConflict = 13,
        MissionAlreadyClaimed = 14,
        InvalidProduction = 15,
        SameProduction = 16,
        MineSlotsFull = 17,
        CartUpgradeLocked = 18,
        CartMaxLevel = 19,
        InvalidGem = 20,
        InvalidStage = 21,
        WorkshopLocked = 22,
        DailyNotFound = 23,
        DailyNotCompleted = 24,
        DailyAlreadyClaimed = 25,
        ChestNotReady = 26,
        ChestAlreadyClaimed = 27,
    }
}
