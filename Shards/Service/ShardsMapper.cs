using Shards.Config;
using Shards.Model.DTO;
using Shards.Model.Res;
using Shards.Rules;

namespace Shards.Service
{
    /// <summary>Converte as entidades do jogo nas respostas da API. Reutilizado por todos os endpoints.</summary>
    public class ShardsMapper(GameRules rules)
    {
        public PlayerRes ToPlayerRes(PlayerDTO player) => new()
        {
            Level = player.Level,
            Experience = player.Experience,
            ExperienceToNextLevel = rules.XpToNextLevel(player.Level, player.Experience),
            SkillPoints = player.SkillPoints,
            Energy = player.Energy,
            MaxEnergy = player.MaxEnergy,
            SecondsPerEnergy = rules.Balance.Energy.SecondsPerPoint,
            NextEnergyAtUtc = rules.NextEnergyAtUtc(player.Energy, player.MaxEnergy, player.LastEnergyUpdateUtc),
        };

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

        /// <summary>Só os minérios que o jogador possui (quantidade maior que zero), em ordem de tipo.</summary>
        public List<OreAmountRes> ToInventoryRes(IEnumerable<InventoryItemDTO> inventory) =>
            inventory
                .Where(i => i.Quantity > 0)
                .OrderBy(i => i.OreType)
                .Select(i => new OreAmountRes { OreType = i.OreType, Quantity = i.Quantity })
                .ToList();

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

            Dictionary<OreType, int> owned = inventory.ToDictionary(i => i.OreType, i => i.Quantity);

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
            Dictionary<OreType, int> owned = inventory.ToDictionary(i => i.OreType, i => i.Quantity);
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
            Dictionary<OreType, int> owned = inventory.ToDictionary(i => i.OreType, i => i.Quantity);

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

        public ShardsStateRes ToStateRes(
            PlayerDTO player,
            IReadOnlyCollection<PlayerSkillDTO> skills,
            IReadOnlyCollection<InventoryItemDTO> inventory,
            IReadOnlyCollection<MineDTO> mines,
            IReadOnlyCollection<PlayerMissionDTO> missions) => new()
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
        };
    }
}
