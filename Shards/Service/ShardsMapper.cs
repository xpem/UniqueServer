using Shards.Config;
using Shards.Model.DTO;
using Shards.Model.Res;
using Shards.Rules;

namespace Shards.Service
{
    /// <summary>Converte as entidades do jogo nas respostas da API. Reutilizado por todos os endpoints.</summary>
    public class ShardsMapper(GameRules rules)
    {
        public PlayerRes ToPlayerRes(PlayerDTO player)
        {
            // a barra de exploração sai regenerada até agora (função pura, sem gravar): as respostas de ação nunca trazem valor velho
            EnergyState explore = rules.RegenerateExploreEnergy(player);

            return new()
            {
                Level = player.Level,
                Experience = player.Experience,
                ExperienceToNextLevel = rules.XpToNextLevel(player.Level, player.Experience),
                SkillPoints = player.SkillPoints,
                Energy = player.Energy,
                MaxEnergy = player.MaxEnergy,
                SecondsPerEnergy = rules.Balance.Energy.SecondsPerPoint,
                NextEnergyAtUtc = rules.NextEnergyAtUtc(player.Energy, player.MaxEnergy, player.LastEnergyUpdateUtc, rules.Balance.Energy.SecondsPerPoint),
                ExploreEnergy = explore.Energy,
                ExploreMaxEnergy = player.ExploreMaxEnergy,
                ExploreSecondsPerEnergy = rules.Balance.Explore.SecondsPerPoint,
                NextExploreEnergyAtUtc = rules.NextEnergyAtUtc(explore.Energy, player.ExploreMaxEnergy, explore.LastUpdateUtc, rules.Balance.Explore.SecondsPerPoint),
            };
        }

        /// <summary>Todas as skills do jogo, com nível 0 para as que o jogador ainda não tem.</summary>
        public List<SkillRes> ToSkillsRes(IEnumerable<PlayerSkillDTO> skills)
        {
            Dictionary<SkillType, int> owned = skills.ToDictionary(s => s.SkillType, s => s.Level);

            return Enum.GetValues<SkillType>()
                .Select(type => new SkillRes
                {
                    Type = type,
                    Level = owned.GetValueOrDefault(type),
                    MaxLevel = rules.MaxSkillLevel(type),
                })
                .ToList();
        }

        /// <summary>Só os minérios brutos que o jogador possui (quantidade maior que zero), em ordem de tipo. Os demais estágios saem em GET /inventory.</summary>
        public List<OreAmountRes> ToInventoryRes(IEnumerable<InventoryItemDTO> inventory) =>
            inventory
                .Where(i => i.Stage == ItemStage.Raw && i.Quantity > 0)
                .OrderBy(i => i.OreType)
                .Select(i => new OreAmountRes { OreType = i.OreType, Quantity = i.Quantity })
                .ToList();

        /// <summary>Inventário completo (todos os estágios), só com quantidade maior que zero, ordenado por minério e depois por estágio.</summary>
        public InventoryRes ToInventoryDetailRes(IEnumerable<InventoryItemDTO> inventory) => new()
        {
            Items = inventory
                .Where(i => i.Quantity > 0)
                .OrderBy(i => i.OreType)
                .ThenBy(i => i.Stage)
                .Select(i => new InventoryItemRes { OreType = i.OreType, Stage = i.Stage, Quantity = i.Quantity })
                .ToList(),
        };

        /// <summary>Saldo em minério bruto por tipo: é o que paga os custos do jogo.</summary>
        private static Dictionary<OreType, int> RawBalance(IEnumerable<InventoryItemDTO> inventory) =>
            inventory.Where(i => i.Stage == ItemStage.Raw).ToDictionary(i => i.OreType, i => i.Quantity);

        public MineStateRes ToMineStateRes(MineDTO mine) => new()
        {
            MineNumber = mine.MineNumber,
            Production = mine.ProductionType,
            CartLevel = mine.CartLevel,
            CartCapacity = mine.CartCapacity,
            CartAmount = rules.CartAmount(mine.CartCapacity, mine.OresPerMinute, mine.LastCartCollectionUtc),
            CartFullAtUtc = rules.CartFullAtUtc(mine.CartCapacity, mine.OresPerMinute, mine.LastCartCollectionUtc),
            OresPerMinute = mine.OresPerMinute,
        };

        public MissionRes ToMissionRes(PlayerMissionDTO mission)
        {
            MissionDefinition definition = MissionCatalog.Get(mission.MissionType);

            return new MissionRes
            {
                Type = mission.MissionType,
                Name = definition.Name,
                Description = definition.Description,
                Status = mission.Status,
                Progress = mission.Progress,
                Goal = definition.Goal,
                Reward = new MissionRewardRes
                {
                    Xp = definition.Reward.Xp,
                    Ores = definition.Reward.Ores.Select(o => new OreAmountRes { OreType = o.Ore, Quantity = o.Quantity }).ToList(),
                },
            };
        }

        /// <summary>Missões visíveis: ativas, ou concluídas ainda não resgatadas, na ordem do catálogo.</summary>
        public List<MissionRes> ToVisibleMissionsRes(IEnumerable<PlayerMissionDTO> missions) =>
            missions
                .Where(m => m.Status != MissionStatus.Claimed)
                .OrderBy(m => m.MissionType)
                .Select(ToMissionRes)
                .ToList();

        /// <summary>Próxima mina à venda (depois da maior que o jogador possui); nulo se não houver.</summary>
        public NextMineRes? ToNextMineRes(IReadOnlyCollection<MineDTO> mines, IEnumerable<InventoryItemDTO> inventory, IEnumerable<PlayerSkillDTO> skills)
        {
            int highestOwned = mines.Select(m => m.MineNumber).DefaultIfEmpty(0).Max();

            var next = rules.GetNextMine(highestOwned);
            if (next is null) return null;

            Dictionary<OreType, int> owned = RawBalance(inventory);

            return new NextMineRes
            {
                MineNumber = next.MineNumber,
                Cost = next.UnlockCost.Select(c => new OreAmountRes { OreType = c.Ore, Quantity = c.Quantity }).ToList(),
                CanAfford = next.UnlockCost.All(c => owned.GetValueOrDefault(c.Ore) >= c.Quantity),
                HasSlot = mines.Count < rules.MineSlotsFor(SkillLevel(skills, SkillType.MineSlots)),
            };
        }

        private static int SkillLevel(IEnumerable<PlayerSkillDTO> skills, SkillType type) =>
            skills.Where(s => s.SkillType == type).Select(s => s.Level).FirstOrDefault();

        /// <summary>Espaços de mina: o total é o maior entre o que a skill libera e as minas que o jogador já tem.</summary>
        public MineSlotsRes ToMineSlotsRes(IReadOnlyCollection<MineDTO> mines, IEnumerable<PlayerSkillDTO> skills) => new()
        {
            Used = mines.Count,
            Total = Math.Max(rules.MineSlotsFor(SkillLevel(skills, SkillType.MineSlots)), mines.Count),
        };

        /// <summary>Por mina: o nível do vagonete e a próxima melhoria (nula no nível máximo).</summary>
        public List<MineCartUpgradeRes> ToCartUpgradesRes(IEnumerable<MineDTO> mines, IEnumerable<InventoryItemDTO> inventory, IEnumerable<PlayerSkillDTO> skills)
        {
            Dictionary<OreType, int> owned = RawBalance(inventory);
            int skillLevel = SkillLevel(skills, SkillType.CartCapacity);

            return mines.OrderBy(m => m.MineNumber).Select(mine =>
            {
                CartUpgradeOptions? next = rules.NextCartUpgrade(mine.CartLevel);

                return new MineCartUpgradeRes
                {
                    MineNumber = mine.MineNumber,
                    CartLevel = mine.CartLevel,
                    Next = next is null ? null : new NextCartUpgradeRes
                    {
                        Level = next.Level,
                        Capacity = next.Capacity,
                        Cost = next.Cost.Select(c => new OreAmountRes { OreType = c.Ore, Quantity = c.Quantity }).ToList(),
                        CanAfford = next.Cost.All(c => owned.GetValueOrDefault(c.Ore) >= c.Quantity),
                        SkillAllows = next.Level <= skillLevel,
                    },
                };
            }).ToList();
        }

        /// <summary>As produções configuradas, com as probabilidades normalizadas e se o inventário cobre o custo.</summary>
        public List<ProductionOptionRes> ToProductionsRes(IEnumerable<InventoryItemDTO> inventory)
        {
            Dictionary<OreType, int> owned = RawBalance(inventory);

            return rules.Balance.Productions.Select(p =>
            {
                double total = p.Drops.Sum(d => d.Chance);

                return new ProductionOptionRes
                {
                    Focus = p.Focus,
                    SwitchCost = p.SwitchCost.Select(c => new OreAmountRes { OreType = c.Ore, Quantity = c.Quantity }).ToList(),
                    Drops = p.Drops.Select(d => new DropRes { OreType = d.Ore, Chance = d.Chance / total, Xp = d.Xp }).ToList(),
                    CanAfford = p.SwitchCost.All(c => owned.GetValueOrDefault(c.Ore) >= c.Quantity),
                };
            }).ToList();
        }

        /// <summary>As receitas da oficina na ordem de nível; o saldo por gema considera cada estágio e o quartzo bruto.</summary>
        public List<WorkshopRecipeRes> ToWorkshopRes(IReadOnlyCollection<InventoryItemDTO> inventory, IEnumerable<PlayerSkillDTO> skills)
        {
            int skillLevel = SkillLevel(skills, SkillType.Workshop);
            IReadOnlyCollection<OreType> gems = rules.CraftableOres;

            return rules.Balance.Workshop.OrderBy(s => s.Level).Select(step =>
            {
                ItemStage source = step.Stage - 1;
                bool canPayCost = InventoryItemService.CanAfford(inventory, step.Cost.Select(c => (c.Ore, c.Quantity)));

                return new WorkshopRecipeRes
                {
                    Stage = step.Stage,
                    Level = step.Level,
                    InputQuantity = step.InputQuantity,
                    Cost = step.Cost.Select(c => new OreAmountRes { OreType = c.Ore, Quantity = c.Quantity }).ToList(),
                    SkillAllows = step.Level <= skillLevel,
                    Gems = gems.Select(gem =>
                    {
                        int available = InventoryItemService.QuantityOf(inventory, gem, source);
                        return new WorkshopGemRes { OreType = gem, Available = available, CanAfford = canPayCost && available >= step.InputQuantity };
                    }).ToList(),
                };
            }).ToList();
        }

        /// <summary>O catálogo de minérios na ordem da configuração.</summary>
        public List<OreInfoRes> ToOresRes() => rules.Balance.Ores.Select(o => new OreInfoRes
        {
            OreType = o.Ore,
            Kind = o.Kind,
            Element = o.Element,
            Craftable = o.Craftable,
            Attack = o.Attack.OrderBy(a => a.Key).Select(a => new OreAttackRes { Stage = a.Key, Attack = a.Value }).ToList(),
            Defense = o.Defense.OrderBy(d => d.Key).Select(d => new OreDefenseRes { Stage = d.Key, Defense = d.Value }).ToList(),
        }).ToList();

        public CombatRes ToCombatRes() => new()
        {
            EnergyPerCounterPoint = rules.Balance.Combat.EnergyPerCounterPoint,
            Effectiveness = rules.Balance.Combat.Effectiveness
                .Select(e => new EffectivenessRes { Attacker = e.Attacker, Defender = e.Defender, Multiplier = e.Multiplier }).ToList(),
        };

        public DailyMissionRes ToDailyMissionRes(DailyMissionDTO mission)
        {
            DailyMissionDefinition definition = DailyMissionCatalog.Get(mission.DailyType);

            return new DailyMissionRes
            {
                Type = mission.DailyType,
                Name = definition.Name,
                Description = definition.Description,
                Status = mission.Status,
                Progress = mission.Progress,
                Goal = definition.Goal,
                Reward = new MissionRewardRes
                {
                    Xp = definition.Reward.Xp,
                    Ores = definition.Reward.Ores.Select(o => new OreAmountRes { OreType = o.Ore, Quantity = o.Quantity }).ToList(),
                },
            };
        }

        /// <summary>Sequência efetiva: vale se o último baú foi hoje ou ontem; senão quebrou.</summary>
        public static int EffectiveStreak(PlayerDTO player, DateOnly day) =>
            player.LastChestDay is { } last && (last == day || last == day.AddDays(-1)) ? player.DailyStreak : 0;

        /// <summary>Dia da sequência que o próximo baú pagaria (hoje, ou amanhã se o de hoje já foi resgatado).</summary>
        public static int NextChestStreakDay(PlayerDTO player, DateOnly day) =>
            player.LastChestDay == day ? player.DailyStreak + 1 : EffectiveStreak(player, day) + 1;

        public DailyStateRes ToDailyStateRes(PlayerDTO player, IEnumerable<DailyMissionDTO> today, DateOnly day)
        {
            List<DailyMissionDTO> rows = today.OrderBy(d => d.DailyType).ToList();
            bool claimed = player.LastChestDay == day;
            (int xp, int quartz) = rules.ChestReward(NextChestStreakDay(player, day));

            return new DailyStateRes
            {
                Day = day.ToString("yyyy-MM-dd"),
                ResetsAtUtc = rules.NextDailyResetUtc,
                Missions = rows.Select(ToDailyMissionRes).ToList(),
                Chest = new DailyChestRes
                {
                    Claimed = claimed,
                    CanClaim = !claimed && rows.Count > 0 && rows.All(r => r.Status == MissionStatus.Claimed),
                    Reward = new MissionRewardRes { Xp = xp, Ores = [new OreAmountRes { OreType = OreType.Quartz, Quantity = quartz }] },
                },
                Streak = new DailyStreakRes { Days = EffectiveStreak(player, day), CycleDays = rules.Balance.Daily.Streak.CycleDays },
            };
        }

        /// <summary>canOpenMore: ainda cabe mais um depósito aberto (MaxOpen). Só importa para o intocado, que precisaria se abrir para ser atacado.</summary>
        public DepositRes ToDepositRes(DepositDTO deposit, bool canOpenMore = true)
        {
            DepositTierOptions? tier = rules.FindTier(deposit.Slot);
            DepositStatus status = rules.EffectiveStatus(deposit);
            double total = tier?.Drops.Sum(d => d.Chance) ?? 0;

            return new DepositRes
            {
                Id = deposit.Id,
                Slot = deposit.Slot,
                Tier = tier?.Name ?? "",
                Element = deposit.Element,
                DefenseCurrent = deposit.DefenseCurrent,
                DefenseMax = deposit.DefenseMax,
                StockRemaining = deposit.StockRemaining,
                StockMax = deposit.StockMax,
                Status = status,
                CanRotate = status == DepositStatus.Sealed,
                CanMine = status == DepositStatus.Open && deposit.DefenseCurrent == 0 && deposit.StockRemaining > 0,
                CanAttack = deposit.DefenseCurrent > 0 && (status == DepositStatus.Open || (status == DepositStatus.Sealed && canOpenMore)),
                CounterRatio = tier?.CounterRatio ?? 0,
                ExpiresAtUtc = status == DepositStatus.Sealed ? rules.NextWeekResetUtc : null,
                Drops = tier is null ? [] : tier.Drops.Select(d => new DropRes { OreType = d.Ore, Chance = d.Chance / total, Xp = d.Xp }).ToList(),
            };
        }

        /// <summary>
        /// Bloco de depósitos: as ofertas da semana (por slot) e os já atacados de semanas anteriores. Intocado de semana passada some.
        /// As trocas somam as linhas da semana corrente.
        /// </summary>
        public DepositsStateRes ToDepositsStateRes(IEnumerable<DepositDTO> deposits, IEnumerable<InventoryItemDTO> inventory)
        {
            DateOnly week = rules.GameWeek;
            List<DepositDTO> rows = deposits.ToList();

            int used = rows.Where(d => d.WeekStart == week).Sum(d => d.RerollCount);
            int freeLeft = Math.Max(0, rules.Balance.Deposits.Reroll.FreePerWeek - used);
            OreAmount? cost = rules.RerollCost(used);
            Dictionary<OreType, int> owned = RawBalance(inventory);
            int openCount = rows.Count(d => d.Status == DepositStatus.Open);
            bool canOpenMore = openCount < rules.Balance.Deposits.MaxOpen;

            return new DepositsStateRes
            {
                WeekStart = week.ToString("yyyy-MM-dd"),
                ResetsAtUtc = rules.NextWeekResetUtc,
                OpenCount = openCount,
                MaxOpen = rules.Balance.Deposits.MaxOpen,
                EnergyPerExtraction = rules.Balance.Deposits.EnergyPerExtraction,
                Rerolls = new DepositRerollsRes
                {
                    Used = used,
                    FreeLeft = freeLeft,
                    NextCost = cost is null ? null : new OreAmountRes { OreType = cost.Ore, Quantity = cost.Quantity },
                    CanAfford = cost is null || owned.GetValueOrDefault(cost.Ore) >= cost.Quantity,
                },
                Items = rows
                    .Where(d => rules.EffectiveStatus(d) != DepositStatus.Expired)
                    .OrderByDescending(d => d.WeekStart)
                    .ThenBy(d => d.Slot)
                    .Select(d => ToDepositRes(d, canOpenMore))
                    .ToList(),
            };
        }

        public ShardsStateRes ToStateRes(
            PlayerDTO player,
            IReadOnlyCollection<PlayerSkillDTO> skills,
            IReadOnlyCollection<InventoryItemDTO> inventory,
            IReadOnlyCollection<MineDTO> mines,
            IReadOnlyCollection<PlayerMissionDTO> missions,
            DailyStateRes? daily = null,
            DepositsStateRes? deposits = null) => new()
        {
            ServerTimeUtc = rules.UtcNow,
            Player = ToPlayerRes(player),
            Skills = ToSkillsRes(skills),
            Inventory = ToInventoryRes(inventory),
            Mines = mines.OrderBy(m => m.MineNumber).Select(ToMineStateRes).ToList(),
            Missions = ToVisibleMissionsRes(missions),
            NextMine = ToNextMineRes(mines, inventory, skills),
            MineSlots = ToMineSlotsRes(mines, skills),
            Productions = ToProductionsRes(inventory),
            CartUpgrades = ToCartUpgradesRes(mines, inventory, skills),
            Workshop = ToWorkshopRes(inventory, skills),
            Ores = ToOresRes(),
            Combat = ToCombatRes(),
            Daily = daily,
            Deposits = deposits,
        };
    }
}
