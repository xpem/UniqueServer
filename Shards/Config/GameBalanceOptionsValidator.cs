using Microsoft.Extensions.Options;

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

                if (mine.Drops.Count == 0) errors.Add($"{at}: Drops não pode ser vazio.");
                if (mine.Drops.Any(d => d.Chance <= 0)) errors.Add($"{at}: toda Chance de Drops deve ser maior que 0.");
                if (mine.Drops.Any(d => d.Xp < 0)) errors.Add($"{at}: Xp de Drops não pode ser negativo.");

                if (mine.UnlockCost.Any(c => c.Quantity <= 0)) errors.Add($"{at}: toda Quantity de UnlockCost deve ser maior que 0.");
            }

            return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
        }
    }
}
