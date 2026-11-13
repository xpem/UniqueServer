using Microsoft.Extensions.Options;
using Shards.Config;
using Shards.Model.DTO;

namespace Shards.Rules
{
    public record EnergyState(int Energy, DateTime LastUpdateUtc);

    public record XpResult(int Level, int Experience, int LevelsGained);

    /// <summary>
    /// Regras determinísticas do jogo, sem acesso a banco. Tempo e sorteio são injetados
    /// (TimeProvider e Random) para os testes não dependerem do relógio nem do acaso.
    /// </summary>
    public class GameRules(IOptions<GameBalanceOptions> options, TimeProvider time, Random random)
    {
        private readonly GameBalanceOptions _balance = options.Value;

        public GameBalanceOptions Balance => _balance;

        public DateTime UtcNow => time.GetUtcNow().UtcDateTime;

        #region Energia

        /// <summary>
        /// Regenera a energia desde lastUpdateUtc. Ao regenerar N pontos o timestamp avança só N * secondsPerPoint,
        /// preservando o resto parcial. Com a energia cheia o timestamp passa a ser agora (a contagem recomeça ao gastar).
        /// Serve às duas barras (normal e de exploração): só mudam o máximo e os segundos por ponto.
        /// </summary>
        public EnergyState RegenerateEnergy(int energy, int maxEnergy, DateTime lastUpdateUtc, int secondsPerPoint)
        {
            DateTime now = UtcNow;

            if (energy >= maxEnergy) return new EnergyState(maxEnergy, now);

            double seconds = (now - lastUpdateUtc).TotalSeconds;

            long points = seconds <= 0 ? 0 : (long)(seconds / secondsPerPoint);
            if (points == 0) return new EnergyState(energy, lastUpdateUtc);

            if (energy + points >= maxEnergy) return new EnergyState(maxEnergy, now);

            return new EnergyState((int)(energy + points), lastUpdateUtc.AddSeconds(points * secondsPerPoint));
        }

        /// <summary>Instante em que o próximo ponto de energia chega; null se a energia está cheia.</summary>
        public DateTime? NextEnergyAtUtc(int energy, int maxEnergy, DateTime lastUpdateUtc, int secondsPerPoint) =>
            energy >= maxEnergy ? null : lastUpdateUtc.AddSeconds(secondsPerPoint);

        /// <summary>Energia normal regenerada até agora.</summary>
        public EnergyState RegenerateMainEnergy(PlayerDTO player) =>
            RegenerateEnergy(player.Energy, player.MaxEnergy, player.LastEnergyUpdateUtc, _balance.Energy.SecondsPerPoint);

        /// <summary>Energia de exploração regenerada até agora (barra própria, ritmo de Explore.SecondsPerPoint).</summary>
        public EnergyState RegenerateExploreEnergy(PlayerDTO player) =>
            RegenerateEnergy(player.ExploreEnergy, player.ExploreMaxEnergy, player.LastExploreEnergyUpdateUtc, _balance.Explore.SecondsPerPoint);

        #endregion

        #region Experiência e nível

        /// <summary>XP acumulado necessário para alcançar o nível informado (nível 0 = 0 XP).</summary>
        public long XpForLevel(int level) =>
            level <= 0 ? 0 : (long)_balance.Experience.XpFirstLevel * level * (level + 1) / 2;

        public int LevelForXp(int experience)
        {
            int level = 0;
            while (XpForLevel(level + 1) <= experience) level++;
            return level;
        }

        public int XpToNextLevel(int level, int experience) => (int)Math.Clamp(XpForLevel(level + 1) - experience, 0, int.MaxValue);

        /// <summary>Soma XP e recalcula o nível. Cada nível ganho vale 1 ponto de habilidade; pode subir mais de um por vez.</summary>
        public XpResult AddExperience(int level, int experience, int gained)
        {
            int newExperience = (int)Math.Min(int.MaxValue, (long)experience + Math.Max(0, gained));
            int newLevel = Math.Max(level, LevelForXp(newExperience));

            return new XpResult(newLevel, newExperience, newLevel - level);
        }

        #endregion

        #region Vagonete

        /// <summary>Minérios acumulados no vagonete: min(capacidade, minutos * taxa), arredondado para baixo.</summary>
        public int CartAmount(int capacity, double oresPerMinute, DateTime lastCollectionUtc)
        {
            double minutes = (UtcNow - lastCollectionUtc).TotalMinutes;
            if (minutes <= 0 || oresPerMinute <= 0) return 0;

            // pequena tolerância para não perder 1 minério por erro de ponto flutuante (ex.: 100 * 0.33)
            double generated = Math.Floor(minutes * oresPerMinute + 1e-9);

            return (int)Math.Min(capacity, generated);
        }

        /// <summary>Instante em que o vagonete enche; null se a mina não gera minérios.</summary>
        public DateTime? CartFullAtUtc(int capacity, double oresPerMinute, DateTime lastCollectionUtc) =>
            oresPerMinute <= 0 ? null : lastCollectionUtc.AddMinutes(capacity / oresPerMinute);

        #endregion

        #region Minas e drops

        public MineOptions GetMine(int mineNumber) =>
            _balance.Mines.FirstOrDefault(m => m.MineNumber == mineNumber)
            ?? throw new KeyNotFoundException($"Mina {mineNumber} não configurada.");

        /// <summary>Próxima mina configurada depois da maior que o jogador já possui; null se não houver.</summary>
        public MineOptions? GetNextMine(int highestOwnedMineNumber) =>
            _balance.Mines.Where(m => m.MineNumber > highestOwnedMineNumber).OrderBy(m => m.MineNumber).FirstOrDefault();

        public ProductionOptions? FindProduction(OreType focus) => _balance.Productions.FirstOrDefault(p => p.Focus == focus);

        public ProductionOptions GetProduction(OreType focus) =>
            FindProduction(focus) ?? throw new KeyNotFoundException($"Produção {focus} não configurada.");

        /// <summary>Sorteia um drop da tabela da produção da mina (pesos normalizados pela soma).</summary>
        public DropOptions RollDrop(ProductionOptions production) => RollDrop(production.Drops);

        /// <summary>Sorteia um drop de qualquer tabela (produção da mina ou faixa de depósito); pesos normalizados pela soma.</summary>
        public DropOptions RollDrop(IReadOnlyList<DropOptions> drops)
        {
            double total = drops.Sum(d => d.Chance);
            double roll = random.NextDouble() * total;

            double accumulated = 0;
            foreach (DropOptions drop in drops)
            {
                accumulated += drop.Chance;
                if (roll < accumulated) return drop;
            }

            return drops[^1];
        }

        #endregion

        #region Missões diárias

        /// <summary>Dia do jogo atual: o dia vira em Daily.ResetHourUtc (não à meia-noite UTC).</summary>
        public DateOnly DailyDay => DateOnly.FromDateTime(UtcNow.AddHours(-_balance.Daily.ResetHourUtc));

        /// <summary>Instante da próxima virada do dia.</summary>
        public DateTime NextDailyResetUtc => DateTime.SpecifyKind(DailyDay.AddDays(1).ToDateTime(TimeOnly.MinValue).AddHours(_balance.Daily.ResetHourUtc), DateTimeKind.Utc);

        /// <summary>Recompensa do baú para o dia N da sequência (1 = primeiro): Quartz + (N do ciclo - 1) * QuartzPerDay, multiplicado no último dia do ciclo.</summary>
        public (int Xp, int Quartz) ChestReward(int streakDay)
        {
            DailyStreakOptions streak = _balance.Daily.Streak;
            int position = (Math.Max(1, streakDay) - 1) % streak.CycleDays + 1;

            int quartz = _balance.Daily.Chest.Quartz + (position - 1) * streak.QuartzPerDay;
            if (position == streak.CycleDays) quartz *= streak.CycleBonusMultiplier;

            return (_balance.Daily.Chest.Xp, quartz);
        }

        #endregion

        #region Depósitos

        /// <summary>
        /// Semana do jogo atual: a segunda-feira da semana a que o DailyDay pertence. Vira na segunda, na mesma hora das diárias
        /// (domingo 02:59 UTC ainda é a semana anterior com reset às 03:00 UTC).
        /// </summary>
        public DateOnly GameWeek => WeekOf(DailyDay);

        /// <summary>Segunda-feira da semana de um dia do jogo.</summary>
        public static DateOnly WeekOf(DateOnly day) => day.AddDays(-(((int)day.DayOfWeek + 6) % 7));

        /// <summary>Instante da próxima virada de semana (a próxima segunda-feira, na hora de reset das diárias).</summary>
        public DateTime NextWeekResetUtc =>
            DateTime.SpecifyKind(GameWeek.AddDays(7).ToDateTime(TimeOnly.MinValue).AddHours(_balance.Daily.ResetHourUtc), DateTimeKind.Utc);

        public DepositTierOptions? FindTier(int slot) => _balance.Deposits.Tiers.FirstOrDefault(t => t.Slot == slot);

        /// <summary>
        /// Custo da próxima troca, dado quantas já foram feitas na semana (somando todos os slots).
        /// As primeiras FreePerWeek são grátis (null); depois a k-ésima paga custa Cost x CostMultiplier^(k-1).
        /// </summary>
        public OreAmount? RerollCost(int rerollsUsedThisWeek)
        {
            DepositRerollOptions reroll = _balance.Deposits.Reroll;
            int paid = rerollsUsedThisWeek - reroll.FreePerWeek;
            if (paid < 0) return null;

            long quantity = reroll.Cost.Quantity;
            for (int i = 0; i < paid && quantity < int.MaxValue; i++) quantity *= reroll.CostMultiplier;

            return new OreAmount { Ore = reroll.Cost.Ore, Quantity = (int)Math.Min(quantity, int.MaxValue) };
        }

        /// <summary>
        /// Situação para o jogador: um depósito intocado (Sealed) de semana passada conta como Expired, sem gravar nada.
        /// Um depósito já atacado (Open) persiste, e Depleted e Expired gravados valem como estão.
        /// </summary>
        public DepositStatus EffectiveStatus(DepositDTO deposit) =>
            deposit.Status == DepositStatus.Sealed && deposit.WeekStart < GameWeek ? DepositStatus.Expired : deposit.Status;

        #endregion

        #region Catálogo de minérios e combate

        public OreOptions? FindOre(OreType ore) => _balance.Ores.FirstOrDefault(o => o.Ore == ore);

        /// <summary>Gemas de mina comum (Kind Gem): as missões de coleção (Collector, FullSet) olham este conjunto.</summary>
        public IReadOnlyCollection<OreType> GemOres => _balance.Ores.Where(o => o.Kind == OreKind.Gem).Select(o => o.Ore).ToList();

        /// <summary>Minérios que a oficina aceita lapidar, na ordem do catálogo.</summary>
        public IReadOnlyCollection<OreType> CraftableOres => _balance.Ores.Where(o => o.Craftable).Select(o => o.Ore).ToList();

        public bool IsCraftable(OreType ore) => FindOre(ore)?.Craftable == true;

        public Element ElementOf(OreType ore) => FindOre(ore)?.Element ?? Element.Neutral;

        /// <summary>Ataque do minério no estágio informado (0 se não ataca: bruto, quartzo ou sem entrada).</summary>
        public int AttackOf(OreType ore, ItemStage stage) => FindOre(ore)?.Attack.GetValueOrDefault(stage) ?? 0;

        /// <summary>Defesa do minério no estágio informado (0 se não defende: bruto, quartzo ou sem entrada).</summary>
        public int DefenseOf(OreType ore, ItemStage stage) => FindOre(ore)?.Defense.GetValueOrDefault(stage) ?? 0;

        /// <summary>
        /// Multiplicador do ataque de um elemento contra a defesa de outro, pela matriz de efetividade
        /// (1,5 = vantagem, 0,75 = desvantagem, 0 = imune). Par ausente na matriz, ou Neutral, vale 1.
        /// </summary>
        public double ElementMultiplier(Element attacker, Element defender) =>
            _balance.Combat.Effectiveness.FirstOrDefault(e => e.Attacker == attacker && e.Defender == defender)?.Multiplier ?? 1.0;

        #endregion

        #region Skills

        public int MaxSkillLevel(Model.DTO.SkillType type) => type switch
        {
            Model.DTO.SkillType.DoubleDrop => _balance.DoubleDrop.MaxLevel,
            Model.DTO.SkillType.MineSlots => _balance.MineSlots.MaxLevel,
            Model.DTO.SkillType.CartCapacity => _balance.CartUpgrades.Select(u => u.Level).DefaultIfEmpty(0).Max(),
            Model.DTO.SkillType.Workshop => _balance.Workshop.Select(s => s.Level).DefaultIfEmpty(0).Max(),
            _ => 0,
        };

        /// <summary>A receita da oficina que produz o estágio informado; null se não houver.</summary>
        public WorkshopStepOptions? FindWorkshopStep(ItemStage target) => _balance.Workshop.FirstOrDefault(s => s.Stage == target);

        /// <summary>A próxima melhoria do vagonete depois do nível informado; null se já está no máximo.</summary>
        public CartUpgradeOptions? NextCartUpgrade(int currentLevel) =>
            _balance.CartUpgrades.Where(u => u.Level > currentLevel).OrderBy(u => u.Level).FirstOrDefault();

        /// <summary>Quantas minas o jogador pode ter: espaços base + nível da skill MineSlots.</summary>
        public int MineSlotsFor(int skillLevel) =>
            _balance.MineSlots.BaseSlots + Math.Clamp(skillLevel, 0, _balance.MineSlots.MaxLevel);

        public double DoubleDropChance(int skillLevel)
        {
            int level = Math.Clamp(skillLevel, 0, _balance.DoubleDrop.MaxLevel);
            return Math.Min(1.0, level * _balance.DoubleDrop.ChancePerLevel);
        }

        public bool RollDoubleDrop(int skillLevel)
        {
            double chance = DoubleDropChance(skillLevel);
            return chance > 0 && random.NextDouble() < chance;
        }

        #endregion
    }
}
