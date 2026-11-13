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
            ValidateCombat(options, errors);
            ValidateOres(options, errors);

            if (options.Explore.MaxEnergy <= 0) errors.Add("Explore.MaxEnergy deve ser maior que 0.");
            if (options.Explore.SecondsPerPoint <= 0) errors.Add("Explore.SecondsPerPoint deve ser maior que 0.");

            ValidateDeposits(options, errors);

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

        /// <summary>A matriz não repete pares, não usa Neutral e não tem multiplicador negativo (0 = imune).</summary>
        private static void ValidateCombat(GameBalanceOptions options, List<string> errors)
        {
            foreach (EffectivenessOptions e in options.Combat.Effectiveness)
            {
                string at = $"Combat.Effectiveness[{e.Attacker} x {e.Defender}]";

                if (e.Attacker == Element.Neutral || e.Defender == Element.Neutral) errors.Add($"{at}: Neutral não entra na matriz (só o quartzo é neutro e ele não luta).");
                if (e.Multiplier < 0) errors.Add($"{at}: Multiplier não pode ser negativo (use 0 para imune).");
            }

            if (options.Combat.EnergyPerCounterPoint <= 0) errors.Add("Combat.EnergyPerCounterPoint deve ser maior que 0.");

            foreach (var duplicated in options.Combat.Effectiveness.GroupBy(e => (e.Attacker, e.Defender)).Where(g => g.Count() > 1))
                errors.Add($"Combat.Effectiveness: o par {duplicated.Key.Attacker} x {duplicated.Key.Defender} está repetido.");
        }

        /// <summary>
        /// Todo OreType tem exatamente uma entrada (esquecer uma ao criar um minério novo falha na subida). Só o Quartz é a moeda.
        /// Quem não é moeda precisa de ataque para Polished e Faceted; gema precisa de elemento; o elemento precisa estar no ciclo.
        /// </summary>
        private static void ValidateOres(GameBalanceOptions options, List<string> errors)
        {
            foreach (var duplicated in options.Ores.GroupBy(o => o.Ore).Where(g => g.Count() > 1))
                errors.Add($"Ores: {duplicated.Key} está repetido.");

            foreach (OreType ore in Enum.GetValues<OreType>())
                if (!options.Ores.Any(o => o.Ore == ore))
                    errors.Add($"Ores: falta a entrada de {ore} (todo minério do jogo precisa estar no catálogo).");

            OreOptions? quartz = options.Ores.FirstOrDefault(o => o.Ore == OreType.Quartz);
            if (quartz is not null && quartz.Kind != OreKind.Currency) errors.Add("Ores: Quartz deve ser do tipo Currency (a moeda).");
            if (options.Ores.Count(o => o.Kind == OreKind.Currency) > 1) errors.Add("Ores: só pode haver uma moeda (Currency).");

            foreach (OreOptions ore in options.Ores)
            {
                string at = $"Ores[{ore.Ore}]";

                if (ore.Kind == OreKind.Currency)
                {
                    if (ore.Element != Element.Neutral) errors.Add($"{at}: a moeda não tem elemento.");
                    if (ore.Craftable) errors.Add($"{at}: a moeda não pode ser lapidada (Craftable).");
                    if (ore.Attack.Count > 0) errors.Add($"{at}: a moeda não ataca.");
                    if (ore.Defense.Count > 0) errors.Add($"{at}: a moeda não defende.");
                    continue;
                }

                if (ore.Attack.ContainsKey(ItemStage.Raw)) errors.Add($"{at}: o estágio Raw não ataca (só Polished e Faceted).");
                if (ore.Defense.ContainsKey(ItemStage.Raw)) errors.Add($"{at}: o estágio Raw não defende (só Polished e Faceted).");

                foreach (ItemStage stage in new[] { ItemStage.Polished, ItemStage.Faceted })
                {
                    if (!ore.Attack.TryGetValue(stage, out int attack) || attack <= 0)
                        errors.Add($"{at}: Attack precisa de um valor maior que 0 para {stage}.");
                    if (!ore.Defense.TryGetValue(stage, out int defense) || defense <= 0)
                        errors.Add($"{at}: Defense precisa de um valor maior que 0 para {stage}.");
                }

                // só o quartzo é neutro; todo outro minério tem elemento, e o elemento precisa existir na matriz (como atacante ou defensor)
                if (ore.Element == Element.Neutral)
                    errors.Add($"{at}: só o quartzo (moeda) pode ser Neutral; todo outro minério precisa de um elemento.");
                else if (!options.Combat.Effectiveness.Any(e => e.Attacker == ore.Element || e.Defender == ore.Element))
                    errors.Add($"{at}: o elemento {ore.Element} precisa aparecer em Combat.Effectiveness.");
            }
        }

        /// <summary>
        /// Depósitos: faixas de 1 a N sem buracos, defesa e variância válidas, troca coerente e tabela de drop só com minérios do catálogo
        /// que não sejam a moeda (um depósito que paga quartzo cria um laço, já que a munição custa quartzo).
        /// </summary>
        private static void ValidateDeposits(GameBalanceOptions options, List<string> errors)
        {
            DepositsOptions deposits = options.Deposits;

            int maxWorkshopLevel = options.Workshop.Select(s => s.Level).DefaultIfEmpty(0).Max();
            if (deposits.UnlockWorkshopLevel < 1 || deposits.UnlockWorkshopLevel > maxWorkshopLevel)
                errors.Add($"Deposits.UnlockWorkshopLevel deve ficar entre 1 e {maxWorkshopLevel} (nível máximo da oficina).");

            if (deposits.MaxOpen < 1) errors.Add("Deposits.MaxOpen deve ser pelo menos 1.");
            if (deposits.StockPerDefense <= 0) errors.Add("Deposits.StockPerDefense deve ser maior que 0.");
            if (deposits.MaxAttackStacks < 1) errors.Add("Deposits.MaxAttackStacks deve ser pelo menos 1.");
            if (deposits.EnergyPerExtraction < 1) errors.Add("Deposits.EnergyPerExtraction deve ser pelo menos 1.");

            if (deposits.Reroll.FreePerWeek < 0) errors.Add("Deposits.Reroll.FreePerWeek não pode ser negativo.");
            if (deposits.Reroll.Cost.Quantity <= 0) errors.Add("Deposits.Reroll.Cost.Quantity deve ser maior que 0.");
            if (deposits.Reroll.CostMultiplier < 1) errors.Add("Deposits.Reroll.CostMultiplier deve ser pelo menos 1.");
            if (options.Ores.FirstOrDefault(o => o.Ore == deposits.Reroll.Cost.Ore)?.Kind != OreKind.Currency)
                errors.Add("Deposits.Reroll.Cost deve ser cobrado na moeda (Quartz).");

            if (deposits.Tiers.Count == 0) errors.Add("Deposits.Tiers não pode ser vazio.");

            List<DepositTierOptions> tiers = deposits.Tiers.OrderBy(t => t.Slot).ToList();
            for (int i = 0; i < tiers.Count; i++)
            {
                DepositTierOptions tier = tiers[i];
                string at = $"Deposits.Tiers[{tier.Slot}]";

                if (tier.Slot != i + 1) errors.Add($"Deposits.Tiers: os slots devem ir de 1 a {tiers.Count} sem repetir nem pular (encontrado {tier.Slot}).");
                if (string.IsNullOrWhiteSpace(tier.Name)) errors.Add($"{at}: Name é obrigatório.");
                if (tier.Defense <= 0) errors.Add($"{at}: Defense deve ser maior que 0.");
                if (tier.DefenseVariance is < 0 or >= 1) errors.Add($"{at}: DefenseVariance deve ficar entre 0 (inclusive) e 1 (exclusive).");
                if (tier.CounterRatio is <= 0 or > 5) errors.Add($"{at}: CounterRatio deve ficar entre 0 (exclusive) e 5 (o contra-ataque já existe na faixa leve).");

                if (tier.Drops.Count == 0) errors.Add($"{at}: Drops não pode ser vazio.");
                if (tier.Drops.Any(d => d.Chance <= 0)) errors.Add($"{at}: toda Chance de Drops deve ser maior que 0.");
                if (tier.Drops.Any(d => d.Xp < 0)) errors.Add($"{at}: Xp de Drops não pode ser negativo.");

                foreach (DropOptions drop in tier.Drops)
                {
                    OreOptions? ore = options.Ores.FirstOrDefault(o => o.Ore == drop.Ore);
                    if (ore is null) errors.Add($"{at}: o minério {drop.Ore} dos Drops não está no catálogo.");
                    else if (ore.Kind == OreKind.Currency) errors.Add($"{at}: a moeda ({drop.Ore}) não pode ser recompensa de depósito (criaria um laço de quartzo).");
                }
            }

            // o elemento de uma oferta sai das gemas do catálogo; sem nenhuma, o sorteio não teria o que escolher
            if (!options.Ores.Any(o => o.Kind == OreKind.Gem)) errors.Add("Deposits: o catálogo precisa de ao menos uma gema (Kind Gem) para sortear o elemento das ofertas.");
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
                if (options.Ores.FirstOrDefault(o => o.Ore == production.Focus)?.Kind == OreKind.Exclusive)
                    errors.Add($"{at}: um minério exclusivo (de depósito) não pode ser o tipo de produção de uma mina.");
            }
        }
    }
}
