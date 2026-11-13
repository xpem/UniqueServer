using Microsoft.EntityFrameworkCore;
using Shards.Config;
using Shards.Errors;
using Shards.Model.DTO;
using Shards.Model.Req;
using Shards.Model.Res;
using Shards.Repo;
using Shards.Rules;

namespace Shards.Service
{
    public interface IDepositService
    {
        /// <summary>
        /// Troca uma oferta intocada da semana por outra no mesmo slot (elemento diferente do atual). A primeira troca da semana
        /// é grátis; as seguintes custam quartzo, cada uma mais que a anterior (somando todos os slots).
        /// </summary>
        Task<RotateDepositRes> RotateAsync(int uid, int depositId);

        /// <summary>
        /// Ataca um depósito com gemas lapidadas (consumidas). O depósito contra-ataca; o que as gemas defensoras não absorvem tira
        /// energia de exploração. A defesa derrubada fica assim e o depósito passa a Open. Com Preview, só calcula (nada é gravado).
        /// </summary>
        Task<AttackDepositRes> AttackAsync(int uid, int depositId, AttackDepositReq? req);

        /// <summary>
        /// Extrai minério de um depósito de defesa já derrubada: 1 ponto de energia de exploração por unidade, cada unidade sorteia a
        /// tabela de drop da faixa. Extrai no máximo o estoque restante; ao zerar, o depósito passa a Depleted.
        /// </summary>
        Task<MineDepositRes> MineAsync(int uid, int depositId, int times);
    }

    public class DepositService(IDbContextFactory<ShardsDbctx> dbFactory, GameRules rules, ShardsMapper mapper) : IDepositService
    {
        public Task<RotateDepositRes> RotateAsync(int uid, int depositId) => ConcurrencyRetry.RunAsync(async () =>
        {
            await using ShardsDbctx ctx = await dbFactory.CreateDbContextAsync();

            PlayerDTO player = await ShardsQueries.GetPlayerAsync(ctx, uid);

            // sempre filtrado pelo dono: o id de outro jogador é "não encontrado"
            DepositDTO deposit = await ctx.Deposit.FirstOrDefaultAsync(d => d.Id == depositId && d.PlayerId == player.Id)
                ?? throw new ShardsException(ShardsErrorCode.DepositNotFound);

            if (rules.EffectiveStatus(deposit) == DepositStatus.Expired) throw new ShardsException(ShardsErrorCode.DepositExpired);
            if (deposit.Status != DepositStatus.Sealed) throw new ShardsException(ShardsErrorCode.DepositNotRotatable);

            DepositTierOptions tier = rules.FindTier(deposit.Slot)
                ?? throw new InvalidOperationException($"Faixa {deposit.Slot} de depósito não configurada.");

            // o limite é da semana toda: soma as trocas dos slots da semana corrente
            DateOnly week = rules.GameWeek;
            List<DepositDTO> weekRows = await ctx.Deposit.Where(d => d.PlayerId == player.Id && d.WeekStart == week).ToListAsync();
            OreAmount? cost = rules.RerollCost(weekRows.Sum(d => d.RerollCount));

            List<InventoryItemDTO> inventory = await ShardsQueries.GetInventoryAsync(ctx, player.Id);
            if (cost is not null) InventoryItemService.Remove(inventory, [(cost.Ore, cost.Quantity)]);

            // a mesma linha é reescrita (outro elemento, nova defesa e estoque); o nº de trocas entra na semente do sorteio
            deposit.RerollCount++;
            DepositRoll roll = DepositTracker.Roll(rules, tier, player.Id, deposit.WeekStart, deposit.RerollCount, exclude: deposit.Element);
            deposit.Element = roll.Element;
            deposit.DefenseMax = roll.Defense;
            deposit.DefenseCurrent = roll.Defense;
            deposit.StockMax = roll.Stock;
            deposit.StockRemaining = roll.Stock;

            await ctx.SaveChangesAsync();

            // o bloco completo: as da semana mais os atacados de semanas anteriores, já com a contagem de trocas nova
            List<DepositDTO> visible = await ShardsQueries.GetDepositsAsync(ctx, player.Id, week);

            bool canOpenMore = visible.Count(d => d.Status == DepositStatus.Open) < rules.Balance.Deposits.MaxOpen;

            return new RotateDepositRes
            {
                Deposit = mapper.ToDepositRes(deposit, canOpenMore),
                Spent = cost is null ? null : new OreAmountRes { OreType = cost.Ore, Quantity = cost.Quantity },
                Inventory = mapper.ToInventoryRes(inventory),
                Deposits = mapper.ToDepositsStateRes(visible, inventory),
            };
        });

        public Task<AttackDepositRes> AttackAsync(int uid, int depositId, AttackDepositReq? req)
        {
            // o corpo é validado antes de abrir o banco: lixo no texto sai como InvalidGem / InvalidStage / InvalidAttack
            List<GemStack> attackers = ParseStacks(req?.Attackers);
            List<GemStack> defenders = ParseStacks(req?.Defenders);
            if (attackers.Count == 0) throw new ShardsException(ShardsErrorCode.InvalidAttack, "Informe ao menos uma pilha de gemas atacantes.");
            bool preview = req?.Preview == true;

            return ConcurrencyRetry.RunAsync(async () =>
            {
                await using ShardsDbctx ctx = await dbFactory.CreateDbContextAsync();
                DateTime now = rules.UtcNow;

                PlayerDTO player = await ShardsQueries.GetPlayerAsync(ctx, uid);

                DepositDTO deposit = await ctx.Deposit.FirstOrDefaultAsync(d => d.Id == depositId && d.PlayerId == player.Id)
                    ?? throw new ShardsException(ShardsErrorCode.DepositNotFound);

                if (rules.EffectiveStatus(deposit) == DepositStatus.Expired) throw new ShardsException(ShardsErrorCode.DepositExpired);
                if (deposit.Status == DepositStatus.Depleted) throw new ShardsException(ShardsErrorCode.DepositDepleted);
                if (deposit.DefenseCurrent <= 0) throw new ShardsException(ShardsErrorCode.DepositDefenseBroken);

                DepositTierOptions tier = rules.FindTier(deposit.Slot)
                    ?? throw new InvalidOperationException($"Faixa {deposit.Slot} de depósito não configurada.");

                AttackOutcome outcome = DepositCombat.Resolve(rules, deposit, tier, attackers, defenders);
                if (outcome.DamageDealt <= 0)
                    throw new ShardsException(ShardsErrorCode.InvalidAttack, "As gemas atacantes não causam dano a este depósito.");

                List<InventoryItemDTO> inventory = await ShardsQueries.GetInventoryAsync(ctx, player.Id);
                EnergyState explore = rules.RegenerateExploreEnergy(player);

                // o que impediria o ataque real; na prévia vira blocker em vez de erro
                ShardsErrorCode? blocker = null;
                if (deposit.Status == DepositStatus.Sealed)
                {
                    int open = await ctx.Deposit.CountAsync(d => d.PlayerId == player.Id && d.Status == DepositStatus.Open);
                    if (open >= rules.Balance.Deposits.MaxOpen) blocker = ShardsErrorCode.TooManyOpenDeposits;
                }

                // as quantidades LISTADAS precisam existir (a mesma gema nas duas listas é somada); o servidor só gasta o que usa
                bool missingGems = attackers.Concat(defenders)
                    .GroupBy(g => (g.Ore, g.Stage))
                    .Any(g => InventoryItemService.QuantityOf(inventory, g.Key.Ore, g.Key.Stage) < g.Sum(x => (long)x.Quantity));
                if (blocker is null && missingGems) blocker = ShardsErrorCode.NotEnoughResources;

                if (blocker is null && explore.Energy < outcome.EnergyCost) blocker = ShardsErrorCode.NotEnoughExploreEnergy;

                if (blocker is not null && !preview) throw new ShardsException(blocker.Value);

                List<PlayerMissionDTO> changedMissions = [];

                if (!preview)
                {
                    foreach (GemStack used in outcome.AttackersUsed.Concat(outcome.DefendersUsed))
                        InventoryItemService.RemoveStage(inventory, used.Ore, used.Stage, used.Quantity);

                    deposit.DefenseCurrent = outcome.DefenseAfter;
                    if (deposit.Status == DepositStatus.Sealed) deposit.Status = DepositStatus.Open;

                    // a energia de exploração é debitada como a normal: o resto parcial do relógio é preservado
                    player.ExploreEnergy = explore.Energy - outcome.EnergyCost;
                    player.LastExploreEnergyUpdateUtc = explore.LastUpdateUtc;
                    player.UpdatedAt = now;

                    // missões: atacar de verdade conta, e derrubar a defesa conclui "Cofre Aberto"
                    List<PlayerMissionDTO> missions = await ShardsQueries.GetMissionsAsync(ctx, player.Id);
                    changedMissions.AddRange(MissionTracker.AdvanceChanged(missions, MissionType.FirstAttack, 1, now));
                    if (outcome.DefenseAfter == 0) changedMissions.AddRange(MissionTracker.AdvanceChanged(missions, MissionType.VaultOpened, 1, now));

                    await ctx.SaveChangesAsync();
                }

                List<DepositDTO> visible = await ShardsQueries.GetDepositsAsync(ctx, player.Id, rules.GameWeek);
                bool canOpenMore = visible.Count(d => d.Status == DepositStatus.Open) < rules.Balance.Deposits.MaxOpen;

                return new AttackDepositRes
                {
                    Preview = preview,
                    CanConfirm = blocker is null,
                    Blocker = blocker?.ToString(),
                    DefenseBefore = outcome.DefenseBefore,
                    DamageDealt = outcome.DamageDealt,
                    DefenseAfter = outcome.DefenseAfter,
                    Broken = outcome.DefenseAfter == 0,
                    AttackersUsed = outcome.AttackersUsed.Select(ToRes).ToList(),
                    DefendersUsed = outcome.DefendersUsed.Select(ToRes).ToList(),
                    CounterAttack = Math.Round(outcome.CounterAttack, 2),
                    Absorbed = Math.Round(outcome.Absorbed, 2),
                    Unabsorbed = Math.Round(outcome.Unabsorbed, 2),
                    EnergyCost = outcome.EnergyCost,
                    ExploreEnergyAfter = preview ? Math.Max(0, explore.Energy - outcome.EnergyCost) : player.ExploreEnergy,
                    Deposit = mapper.ToDepositRes(deposit, canOpenMore),
                    Inventory = mapper.ToInventoryDetailRes(inventory),
                    Player = mapper.ToPlayerRes(player),
                    Deposits = mapper.ToDepositsStateRes(visible, inventory),
                    Missions = changedMissions.Select(mapper.ToMissionRes).ToList(),
                };
            });
        }

        public Task<MineDepositRes> MineAsync(int uid, int depositId, int times)
        {
            int maxTimes = rules.Balance.Energy.MaxMinesPerRequest;
            if (times < 1 || times > maxTimes)
                throw new ShardsException(ShardsErrorCode.InvalidTimes, $"times deve ficar entre 1 e {maxTimes}.");

            return ConcurrencyRetry.RunAsync(async () =>
            {
                await using ShardsDbctx ctx = await dbFactory.CreateDbContextAsync();
                DateTime now = rules.UtcNow;

                PlayerDTO player = await ShardsQueries.GetPlayerAsync(ctx, uid);

                DepositDTO deposit = await ctx.Deposit.FirstOrDefaultAsync(d => d.Id == depositId && d.PlayerId == player.Id)
                    ?? throw new ShardsException(ShardsErrorCode.DepositNotFound);

                if (rules.EffectiveStatus(deposit) == DepositStatus.Expired) throw new ShardsException(ShardsErrorCode.DepositExpired);
                if (deposit.Status == DepositStatus.Depleted || deposit.StockRemaining <= 0) throw new ShardsException(ShardsErrorCode.DepositDepleted);
                if (deposit.DefenseCurrent > 0) throw new ShardsException(ShardsErrorCode.DepositDefenseActive);

                DepositTierOptions tier = rules.FindTier(deposit.Slot)
                    ?? throw new InvalidOperationException($"Faixa {deposit.Slot} de depósito não configurada.");

                // extrai no máximo o que resta; a energia cobrada é só das unidades extraídas
                int units = Math.Min(times, deposit.StockRemaining);
                int cost = units * rules.Balance.Deposits.EnergyPerExtraction;

                EnergyState explore = rules.RegenerateExploreEnergy(player);
                if (explore.Energy < cost) throw new ShardsException(ShardsErrorCode.NotEnoughExploreEnergy);

                // 1 sorteio da tabela da faixa por unidade (a skill DoubleDrop vale só na mineração ativa)
                SortedDictionary<OreType, int> drops = [];
                int xpGained = 0;
                for (int i = 0; i < units; i++)
                {
                    DropOptions drop = rules.RollDrop(tier.Drops);
                    drops[drop.Ore] = drops.GetValueOrDefault(drop.Ore) + 1;
                    xpGained += drop.Xp;
                }

                List<InventoryItemDTO> inventory = await ShardsQueries.GetInventoryAsync(ctx, player.Id);
                foreach ((OreType ore, int quantity) in drops)
                    InventoryItemService.Add(ctx, inventory, player.Id, ore, quantity);

                XpResult xp = rules.AddExperience(player.Level, player.Experience, xpGained);
                player.Level = xp.Level;
                player.Experience = xp.Experience;
                player.SkillPoints += xp.LevelsGained;

                // a energia de exploração é debitada como a normal: o resto parcial do relógio é preservado
                player.ExploreEnergy = explore.Energy - cost;
                player.LastExploreEnergyUpdateUtc = explore.LastUpdateUtc;
                player.UpdatedAt = now;

                deposit.StockRemaining -= units;
                bool depleted = deposit.StockRemaining == 0;
                if (depleted) deposit.Status = DepositStatus.Depleted;

                // missão Extrator: conta as unidades extraídas
                List<PlayerMissionDTO> missions = await ShardsQueries.GetMissionsAsync(ctx, player.Id);
                List<PlayerMissionDTO> changedMissions = MissionTracker.AdvanceChanged(missions, MissionType.Extractor, units, now);

                await ctx.SaveChangesAsync();

                List<DepositDTO> visible = await ShardsQueries.GetDepositsAsync(ctx, player.Id, rules.GameWeek);
                bool canOpenMore = visible.Count(d => d.Status == DepositStatus.Open) < rules.Balance.Deposits.MaxOpen;

                return new MineDepositRes
                {
                    Units = units,
                    Drops = drops.Select(d => new OreAmountRes { OreType = d.Key, Quantity = d.Value }).ToList(),
                    XpGained = xpGained,
                    LevelUp = xp.LevelsGained > 0,
                    EnergyCost = cost,
                    Depleted = depleted,
                    Deposit = mapper.ToDepositRes(deposit, canOpenMore),
                    Inventory = mapper.ToInventoryRes(inventory),
                    Player = mapper.ToPlayerRes(player),
                    Deposits = mapper.ToDepositsStateRes(visible, inventory),
                    Missions = changedMissions.Select(mapper.ToMissionRes).ToList(),
                };
            });
        }

        private static GemStackRes ToRes(GemStack stack) => new() { OreType = stack.Ore, Stage = stack.Stage, Quantity = stack.Quantity };

        /// <summary>
        /// Converte as pilhas do corpo: minério e estágio pelos parsers do jogo (recusam quartzo e gema bruta), quantidade maior que 0,
        /// pilhas repetidas somadas (a ordem da primeira aparição vale) e no máximo Deposits.MaxAttackStacks por lista.
        /// </summary>
        private List<GemStack> ParseStacks(List<GemStackReq>? items)
        {
            List<GemStack> result = [];

            foreach (GemStackReq item in items ?? [])
            {
                OreType ore = ShardsErrors.ParseGem(item.Ore);
                ItemStage stage = ShardsErrors.ParseCraftStage(item.Stage);
                if (item.Quantity < 1) throw new ShardsException(ShardsErrorCode.InvalidAttack, "A quantidade de cada pilha deve ser maior que 0.");

                int index = result.FindIndex(g => g.Ore == ore && g.Stage == stage);
                if (index >= 0) result[index] = result[index] with { Quantity = (int)Math.Min(int.MaxValue, (long)result[index].Quantity + item.Quantity) };
                else result.Add(new GemStack(ore, stage, item.Quantity));
            }

            int max = rules.Balance.Deposits.MaxAttackStacks;
            if (result.Count > max) throw new ShardsException(ShardsErrorCode.InvalidAttack, $"No máximo {max} pilhas de gemas por lista.");

            return result;
        }
    }
}
