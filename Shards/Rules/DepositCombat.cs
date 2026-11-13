using Shards.Config;
using Shards.Model.DTO;

namespace Shards.Rules
{
    /// <summary>Uma pilha de gemas lapidadas usada num ataque (minério, estágio e quantidade).</summary>
    public record GemStack(OreType Ore, ItemStage Stage, int Quantity);

    /// <summary>
    /// O resultado de um ataque, calculado sem gravar nada (serve à prévia e ao ataque real).
    /// O contra-ataque e a absorção são em pontos (fracionários por causa dos multiplicadores elementais);
    /// a energia perdida já vem arredondada para cima.
    /// </summary>
    public record AttackOutcome(
        int DefenseBefore,
        int DamageDealt,
        int DefenseAfter,
        IReadOnlyList<GemStack> AttackersUsed,
        double CounterAttack,
        double Absorbed,
        double Unabsorbed,
        IReadOnlyList<GemStack> DefendersUsed,
        int EnergyCost);

    /// <summary>
    /// Regras puras do ataque a um depósito (sem banco, sem sorteio): o dano das gemas atacantes, o contra-ataque do depósito,
    /// a absorção pelas gemas defensoras e a energia de exploração perdida. Tudo é determinístico, então a prévia é exata.
    /// </summary>
    public static class DepositCombat
    {
        // pequena tolerância para não gastar uma gema a mais por erro de ponto flutuante (ex.: 3 x 1,5)
        private const double Epsilon = 1e-9;

        /// <summary>
        /// Dano: aplica os atacantes na ordem em que vieram e PARA quando a defesa chega a zero, consumindo só as unidades necessárias
        /// (o excesso da última unidade se perde). Cada unidade causa ataque do estágio x multiplicador (gema contra depósito).
        /// Contra-ataque: dano efetivo x CounterRatio da faixa. Absorção: os defensores, na ordem, só até cobrir o contra-ataque;
        /// cada unidade absorve defesa do estágio / multiplicador (depósito contra gema), então a fraqueza elemental absorve menos.
        /// O que não foi absorvido vira energia de exploração (EnergyPerCounterPoint por ponto, arredondado para cima).
        /// </summary>
        public static AttackOutcome Resolve(
            GameRules rules,
            DepositDTO deposit,
            DepositTierOptions tier,
            IReadOnlyList<GemStack> attackers,
            IReadOnlyList<GemStack> defenders)
        {
            // dano
            double remaining = deposit.DefenseCurrent;
            double damage = 0;
            List<GemStack> attackersUsed = [];

            foreach (GemStack stack in attackers)
            {
                if (remaining <= Epsilon) break;

                double perUnit = rules.AttackOf(stack.Ore, stack.Stage) * rules.ElementMultiplier(rules.ElementOf(stack.Ore), deposit.Element);
                if (perUnit <= 0) continue;

                int units = (int)Math.Min(stack.Quantity, Math.Max(1, Math.Ceiling(remaining / perUnit - Epsilon)));
                attackersUsed.Add(stack with { Quantity = units });

                damage += units * perUnit;
                remaining -= units * perUnit;
            }

            // o dano total sobe para o inteiro seguinte (favorece o jogador em menos de 1 ponto e evita a defesa "presa" em 1)
            int dealt = (int)Math.Min(deposit.DefenseCurrent, Math.Ceiling(damage - Epsilon));
            if (dealt < 0) dealt = 0;

            // contra-ataque e absorção
            double counter = dealt * tier.CounterRatio;
            double stillToAbsorb = counter;
            List<GemStack> defendersUsed = [];

            foreach (GemStack stack in defenders)
            {
                if (stillToAbsorb <= Epsilon) break;

                int defense = rules.DefenseOf(stack.Ore, stack.Stage);
                if (defense <= 0) continue;

                double multiplier = rules.ElementMultiplier(deposit.Element, rules.ElementOf(stack.Ore));

                if (multiplier <= 0)
                {
                    // imune ao elemento do depósito: uma unidade absorve tudo
                    defendersUsed.Add(stack with { Quantity = 1 });
                    stillToAbsorb = 0;
                    break;
                }

                double perUnit = defense / multiplier;
                int units = (int)Math.Min(stack.Quantity, Math.Max(1, Math.Ceiling(stillToAbsorb / perUnit - Epsilon)));
                defendersUsed.Add(stack with { Quantity = units });

                stillToAbsorb -= Math.Min(stillToAbsorb, units * perUnit);
            }

            double unabsorbed = stillToAbsorb <= Epsilon ? 0 : stillToAbsorb;
            int energy = unabsorbed == 0 ? 0 : (int)Math.Ceiling(unabsorbed * rules.Balance.Combat.EnergyPerCounterPoint - Epsilon);

            return new AttackOutcome(
                DefenseBefore: deposit.DefenseCurrent,
                DamageDealt: dealt,
                DefenseAfter: deposit.DefenseCurrent - dealt,
                AttackersUsed: attackersUsed,
                CounterAttack: counter,
                Absorbed: counter - unabsorbed,
                Unabsorbed: unabsorbed,
                DefendersUsed: defendersUsed,
                EnergyCost: energy);
        }
    }
}
