using Microsoft.Extensions.Options;
using Shards.Config;

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
        /// Regenera a energia desde lastUpdateUtc. Ao regenerar N pontos o timestamp avança só N * SecondsPerPoint,
        /// preservando o resto parcial. Com a energia cheia o timestamp passa a ser agora (a contagem recomeça ao gastar).
        /// </summary>
        public EnergyState RegenerateEnergy(int energy, int maxEnergy, DateTime lastUpdateUtc)
        {
            DateTime now = UtcNow;

            if (energy >= maxEnergy) return new EnergyState(maxEnergy, now);

            double seconds = (now - lastUpdateUtc).TotalSeconds;
            int secondsPerPoint = _balance.Energy.SecondsPerPoint;

            long points = seconds <= 0 ? 0 : (long)(seconds / secondsPerPoint);
            if (points == 0) return new EnergyState(energy, lastUpdateUtc);

            if (energy + points >= maxEnergy) return new EnergyState(maxEnergy, now);

            return new EnergyState((int)(energy + points), lastUpdateUtc.AddSeconds(points * secondsPerPoint));
        }

        /// <summary>Instante em que o próximo ponto de energia chega; null se a energia está cheia.</summary>
        public DateTime? NextEnergyAtUtc(int energy, int maxEnergy, DateTime lastUpdateUtc) =>
            energy >= maxEnergy ? null : lastUpdateUtc.AddSeconds(_balance.Energy.SecondsPerPoint);

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

        /// <summary>Sorteia um drop da tabela da mina (pesos normalizados pela soma).</summary>
        public DropOptions RollDrop(MineOptions mine)
        {
            double total = mine.Drops.Sum(d => d.Chance);
            double roll = random.NextDouble() * total;

            double accumulated = 0;
            foreach (DropOptions drop in mine.Drops)
            {
                accumulated += drop.Chance;
                if (roll < accumulated) return drop;
            }

            return mine.Drops[^1];
        }

        #endregion

        #region Skills

        public int MaxSkillLevel(Model.DTO.SkillType type) => type switch
        {
            Model.DTO.SkillType.DoubleDrop => _balance.DoubleDrop.MaxLevel,
            _ => 0,
        };

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
