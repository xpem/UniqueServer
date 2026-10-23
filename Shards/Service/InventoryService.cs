using Microsoft.EntityFrameworkCore;
using Shards.Model.DTO;
using Shards.Model.Res;
using Shards.Repo;

namespace Shards.Service
{
    public interface IInventoryService
    {
        /// <summary>Inventário completo (todos os estágios). Quem ainda não tem jogador recebe a lista vazia; o jogador só é criado em GET /state.</summary>
        Task<InventoryRes> GetAsync(int uid);
    }

    public class InventoryService(IDbContextFactory<ShardsDbctx> dbFactory, ShardsMapper mapper) : IInventoryService
    {
        public async Task<InventoryRes> GetAsync(int uid)
        {
            await using ShardsDbctx ctx = await dbFactory.CreateDbContextAsync();

            int? playerId = await ctx.Player.Where(p => p.UserId == uid).Select(p => (int?)p.Id).FirstOrDefaultAsync();
            if (playerId is null) return new InventoryRes();

            List<InventoryItemDTO> inventory = await ctx.InventoryItem.AsNoTracking().Where(i => i.PlayerId == playerId).ToListAsync();

            return mapper.ToInventoryDetailRes(inventory);
        }
    }
}
