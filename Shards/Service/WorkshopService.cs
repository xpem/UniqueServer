using Microsoft.EntityFrameworkCore;
using Shards.Config;
using Shards.Errors;
using Shards.Model.DTO;
using Shards.Model.Res;
using Shards.Repo;
using Shards.Rules;

namespace Shards.Service
{
    public interface IWorkshopService
    {
        /// <summary>
        /// Lapida gemas: gasta gemas do estágio anterior e minério bruto e produz gemas do estágio pedido (instantâneo e sem risco).
        /// Exige a skill Workshop no nível da receita.
        /// </summary>
        Task<CraftRes> CraftAsync(int uid, string? gem, string? stage, int times);
    }

    public class WorkshopService(IDbContextFactory<ShardsDbctx> dbFactory, GameRules rules, ShardsMapper mapper) : IWorkshopService
    {
        public Task<CraftRes> CraftAsync(int uid, string? gem, string? stage, int times)
        {
            OreType ore = ShardsErrors.ParseGem(gem);
            ItemStage target = ShardsErrors.ParseCraftStage(stage);
            WorkshopStepOptions step = rules.FindWorkshopStep(target) ?? throw new ShardsException(ShardsErrorCode.InvalidStage);

            int maxTimes = rules.Balance.Energy.MaxMinesPerRequest;
            if (times < 1 || times > maxTimes)
                throw new ShardsException(ShardsErrorCode.InvalidTimes, $"times deve ficar entre 1 e {maxTimes}.");

            ItemStage source = target - 1;
            int inputTotal = step.InputQuantity * times;
            List<(OreType Ore, int Quantity)> cost = step.Cost.Select(c => (c.Ore, c.Quantity * times)).ToList();

            return ConcurrencyRetry.RunAsync(async () =>
            {
                await using ShardsDbctx ctx = await dbFactory.CreateDbContextAsync();

                PlayerDTO player = await ShardsQueries.GetPlayerAsync(ctx, uid);

                // a skill dá a permissão; sem ela, nem chega a cobrar
                int skillLevel = await ShardsQueries.GetSkillLevelAsync(ctx, player.Id, SkillType.Workshop);
                if (skillLevel < step.Level) throw new ShardsException(ShardsErrorCode.WorkshopLocked);

                // confere tudo antes de debitar qualquer coisa: faltando algo, nada é gravado
                List<InventoryItemDTO> inventory = await ShardsQueries.GetInventoryAsync(ctx, player.Id);
                if (InventoryItemService.QuantityOf(inventory, ore, source) < inputTotal || !InventoryItemService.CanAfford(inventory, cost))
                    throw new ShardsException(ShardsErrorCode.NotEnoughResources);

                InventoryItemService.RemoveStage(inventory, ore, source, inputTotal);
                InventoryItemService.Remove(inventory, cost);
                InventoryItemService.Add(ctx, inventory, player.Id, ore, times, target);

                // missões da oficina: as de condição (FirstPolish, FirstFacet, FullSet) e as de contagem (PolishBatch, FacetBatch)
                DateTime now = rules.UtcNow;
                List<PlayerMissionDTO> missions = await ShardsQueries.GetMissionsAsync(ctx, player.Id);
                List<PlayerMissionDTO> changedMissions = MissionTracker.CompleteSatisfiedChanged(
                    missions, [MissionType.FirstPolish, MissionType.FirstFacet, MissionType.FullSet], new MissionFacts(Inventory: inventory), now);
                changedMissions.AddRange(MissionTracker.AdvanceChanged(
                    missions, target == ItemStage.Polished ? MissionType.PolishBatch : MissionType.FacetBatch, times, now));

                // diárias: lapidações do dia
                List<DailyMissionDTO> today = await ShardsQueries.GetDailyAsync(ctx, player.Id, rules.DailyDay);
                List<DailyMissionDTO> changedDaily = DailyTracker.AdvanceChanged(
                    today, target == ItemStage.Polished ? DailyMissionType.Polish : DailyMissionType.Facet, times, now);

                await ctx.SaveChangesAsync();

                List<InventoryItemRes> spent = [new() { OreType = ore, Stage = source, Quantity = inputTotal }];
                spent.AddRange(cost.Select(c => new InventoryItemRes { OreType = c.Ore, Stage = ItemStage.Raw, Quantity = c.Quantity }));

                return new CraftRes
                {
                    Crafted = new InventoryItemRes { OreType = ore, Stage = target, Quantity = times },
                    Spent = spent,
                    Inventory = mapper.ToInventoryDetailRes(inventory),
                    Missions = changedMissions.Select(mapper.ToMissionRes).ToList(),
                    DailyMissions = changedDaily.Select(mapper.ToDailyMissionRes).ToList(),
                };
            });
        }
    }
}
