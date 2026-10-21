using Microsoft.Extensions.Options;
using Shards.Model.DTO;

namespace Shards.Config
{
    /// <summary>
    /// Valida o balanceamento na subida da API (ValidateOnStart): uma configuração incompleta ou inválida
    /// derruba a inicialização com uma mensagem clara, em vez de estourar 500 no primeiro jogador.
    /// </summary>
    public class GameBalanceOptionsValidator : IValidateOptions<GameBalanceOptions>
    {
        public ValidateOptionsResult Validate(string? name, GameBalanceOptions options)
        {
            List<string> errors = [];

            if (options.Energy.MaxEnergy <= 0) errors.Add("Energy.MaxEnergy deve ser maior que 0.");
            if (options.Energy.CostPerMine <= 0) errors.Add("Energy.CostPerMine deve ser maior que 0.");
            if (options.Energy.SecondsPerPoint <= 0) errors.Add("Energy.SecondsPerPoint deve ser maior que 0.");
            if (options.Energy.MaxMinesPerRequest <= 0) errors.Add("Energy.MaxMinesPerRequest deve ser maior que 0.");

            if (options.Experience.XpFirstLevel <= 0) errors.Add("Experience.XpFirstLevel deve ser maior que 0.");

            if (options.DoubleDrop.MaxLevel < 0) errors.Add("DoubleDrop.MaxLevel não pode ser negativo.");
            if (options.DoubleDrop.ChancePerLevel is < 0 or > 1) errors.Add("DoubleDrop.ChancePerLevel deve ficar entre 0 e 1.");

            if (options.MineSlots.BaseSlots < 1) errors.Add("MineSlots.BaseSlots deve ser pelo menos 1.");
            if (options.MineSlots.MaxLevel < 0) errors.Add("MineSlots.MaxLevel não pode ser negativo.");

            ValidateMines(options, errors);
            ValidateProductions(options, errors);
            ValidateCartUpgrades(options, errors);
            ValidateWorkshop(options, errors);

            if (options.Daily.ResetHourUtc is < 0 or > 23) errors.Add("Daily.ResetHourUtc deve ficar entre 0 e 23.");
            if (options.Daily.MissionsPerDay is < 1 or > 7) errors.Add("Daily.MissionsPerDay deve ficar entre 1 e 7.");
            if (options.Daily.Chest.Xp < 0 || options.Daily.Chest.Quartz < 0) errors.Add("Daily.Chest não pode ter valores negativos.");
            if (options.Daily.Streak.CycleDays < 1) errors.Add("Daily.Streak.CycleDays deve ser pelo menos 1.");
            if (options.Daily.Streak.QuartzPerDay < 0) errors.Add("Daily.Streak.QuartzPerDay não pode ser negativo.");
            if (options.Daily.Streak.CycleBonusMultiplier < 1) errors.Add("Daily.Streak.CycleBonusMultiplier deve ser pelo menos 1.");

            return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
        }

        private static void ValidateMines(GameBalanceOptions options, List<string> errors)
        {
            if (options.Mines.Count == 0) errors.Add("Mines não pode ser vazio.");
            if (!options.Mines.Any(m => m.MineNumber == 1)) errors.Add("A mina inicial (MineNumber 1) precisa estar configurada.");

            foreach (var duplicated in options.Mines.GroupBy(m => m.MineNumber).Where(g => g.Count() > 1))
                errors.Add($"MineNumber {duplicated.Key} está repetido em Mines.");

            foreach (MineOptions mine in options.Mines)
            {
                string at = $"Mines[{mine.MineNumber}]";

                if (mine.MineNumber < 1) errors.Add($"{at}: MineNumber deve ser maior que 0.");
                if (mine.CartCapacity <= 0) errors.Add($"{at}: CartCapacity deve ser maior que 0.");
                if (mine.OresPerMinute < 0) errors.Add($"{at}: OresPerMinute não pode ser negativo.");

                if (mine.UnlockCost.Any(c => c.Quantity <= 0)) errors.Add($"{at}: toda Quantity de UnlockCost deve ser maior que 0.");
            }
        }

        /// <summary>Os níveis vão de 1 a N sem buracos e a capacidade sempre cresce de um nível para o outro.</summary>
        private static void ValidateCartUpgrades(GameBalanceOptions options, List<string> errors)
        {
            List<CartUpgradeOptions> upgrades = options.CartUpgrades.OrderBy(u => u.Level).ToList();

            for (int i = 0; i < upgrades.Count; i++)
            {
                CartUpgradeOptions upgrade = upgrades[i];

                if (upgrade.Level != i + 1) errors.Add($"CartUpgrades: os níveis devem ir de 1 a {upgrades.Count} sem repetir nem pular (encontrado {upgrade.Level}).");
                if (upgrade.Capacity <= 0) errors.Add($"CartUpgrades[{upgrade.Level}]: Capacity deve ser maior que 0.");
                if (i > 0 && upgrade.Capacity <= upgrades[i - 1].Capacity) errors.Add($"CartUpgrades[{upgrade.Level}]: Capacity deve ser maior que a do nível anterior.");
                if (upgrade.Cost.Any(c => c.Quantity <= 0)) errors.Add($"CartUpgrades[{upgrade.Level}]: toda Quantity de Cost deve ser maior que 0.");
            }
        }

        /// <summary>Os níveis vão de 1 a N sem buracos e cada nível produz o estágio de mesmo número (1 = Polished, 2 = Faceted).</summary>
        private static void ValidateWorkshop(GameBalanceOptions options, List<string> errors)
        {
            List<WorkshopStepOptions> steps = options.Workshop.OrderBy(s => s.Level).ToList();

            for (int i = 0; i < steps.Count; i++)
            {
                WorkshopStepOptions step = steps[i];
                string at = $"Workshop[{step.Level}]";

                if (step.Level != i + 1) errors.Add($"Workshop: os níveis devem ir de 1 a {steps.Count} sem repetir nem pular (encontrado {step.Level}).");
                if (step.Stage == ItemStage.Raw || (int)step.Stage != step.Level) errors.Add($"{at}: Stage deve ser o estágio de mesmo número do nível (1 = Polished, 2 = Faceted).");
                if (step.InputQuantity <= 0) errors.Add($"{at}: InputQuantity deve ser maior que 0.");
                if (step.Cost.Any(c => c.Quantity <= 0)) errors.Add($"{at}: toda Quantity de Cost deve ser maior que 0.");
            }
        }

        /// <summary>Toda mina nasce produzindo Quartzo; as demais produções (gemas) são opções que o jogador compra por mina.</summary>
        private static void ValidateProductions(GameBalanceOptions options, List<string> errors)
        {
            if (options.Productions.Count == 0) errors.Add("Productions não pode ser vazio.");
            if (!options.Productions.Any(p => p.Focus == OreType.Quartz)) errors.Add("A produção Quartz (padrão de toda mina) precisa estar configurada.");

            foreach (var duplicated in options.Productions.GroupBy(p => p.Focus).Where(g => g.Count() > 1))
                errors.Add($"A produção {duplicated.Key} está repetida em Productions.");

            foreach (ProductionOptions production in options.Productions)
            {
                string at = $"Productions[{production.Focus}]";

                if (production.Drops.Count == 0) errors.Add($"{at}: Drops não pode ser vazio.");
                if (production.Drops.Any(d => d.Chance <= 0)) errors.Add($"{at}: toda Chance de Drops deve ser maior que 0.");
                if (production.Drops.Any(d => d.Xp < 0)) errors.Add($"{at}: Xp de Drops não pode ser negativo.");
                if (production.Drops.Count > 0 && !production.Drops.Any(d => d.Ore == production.Focus))
                    errors.Add($"{at}: os Drops precisam incluir o próprio minério da produção.");

                if (production.SwitchCost.Any(c => c.Quantity <= 0)) errors.Add($"{at}: toda Quantity de SwitchCost deve ser maior que 0.");
            }
        }
    }
}
