using Microsoft.EntityFrameworkCore;
using Shards.Errors;
using Shards.Model.DTO;
using Shards.Repo;

namespace Shards.Service
{
    /// <summary>Buscas comuns dos comandos do jogo. As entidades voltam rastreadas, para o comando alterá-las e gravar.</summary>
    public static class ShardsQueries
    {
        public static async Task<PlayerDTO> GetPlayerAsync(ShardsDbctx ctx, int uid) =>
            await ctx.Player.FirstOrDefaultAsync(p => p.UserId == uid)
            ?? throw new ShardsException(ShardsErrorCode.PlayerNotFound);

        public static async Task<MineDTO> GetMineAsync(ShardsDbctx ctx, int playerId, int mineNumber) =>
            await ctx.Mine.FirstOrDefaultAsync(m => m.PlayerId == playerId && m.MineNumber == mineNumber)
            ?? throw new ShardsException(ShardsErrorCode.MineNotFound);

        public static Task<List<InventoryItemDTO>> GetInventoryAsync(ShardsDbctx ctx, int playerId) =>
            ctx.InventoryItem.Where(i => i.PlayerId == playerId).ToListAsync();

        public static Task<List<PlayerMissionDTO>> GetMissionsAsync(ShardsDbctx ctx, int playerId) =>
            ctx.PlayerMission.Where(m => m.PlayerId == playerId).ToListAsync();

        public static async Task<int> GetSkillLevelAsync(ShardsDbctx ctx, int playerId, SkillType type) =>
            await ctx.PlayerSkill
                .Where(s => s.PlayerId == playerId && s.SkillType == type)
                .Select(s => s.Level)
                .FirstOrDefaultAsync();
    }
}
