using Shards.Config;
using Shards.Model.DTO;
using Shards.Rules;

namespace Shards.Service
{
    /// <summary>O que o sorteio de uma oferta decide: elemento, defesa e estoque.</summary>
    public record DepositRoll(Element Element, int Defense, int Stock);

    /// <summary>Cria e sorteia as ofertas semanais de depósitos (entidades já rastreadas pelo DbContext, sem acesso a banco).</summary>
    public static class DepositTracker
    {
        /// <summary>Os depósitos só existem depois que a oficina está liberada (skill Workshop no nível configurado).</summary>
        public static bool IsUnlocked(GameRules rules, int workshopLevel) =>
            workshopLevel >= rules.Balance.Deposits.UnlockWorkshopLevel;

        /// <summary>
        /// Cria as ofertas que faltam na semana, uma por faixa (slot). O sorteio é determinístico por (jogador, semana, slot, nº de trocas):
        /// duas leituras simultâneas geram a mesma oferta, e a que perder a corrida cai no índice único.
        /// </summary>
        public static List<DepositDTO> CreateMissing(IReadOnlyCollection<DepositDTO> thisWeek, int playerId, DateOnly week, GameRules rules, DateTime nowUtc)
        {
            List<DepositDTO> created = [];

            foreach (DepositTierOptions tier in rules.Balance.Deposits.Tiers.OrderBy(t => t.Slot))
            {
                if (thisWeek.Any(d => d.Slot == tier.Slot)) continue;

                DepositRoll roll = Roll(rules, tier, playerId, week, rerollCount: 0, exclude: null);

                created.Add(new DepositDTO
                {
                    PlayerId = playerId,
                    Slot = tier.Slot,
                    WeekStart = week,
                    Element = roll.Element,
                    DefenseMax = roll.Defense,
                    DefenseCurrent = roll.Defense,
                    StockMax = roll.Stock,
                    StockRemaining = roll.Stock,
                    CreatedAt = nowUtc,
                });
            }

            return created;
        }

        /// <summary>
        /// Sorteia uma oferta. O elemento sai das gemas do catálogo; numa troca (exclude) o elemento atual fica de fora,
        /// para o jogador nunca pagar por uma troca que não muda nada (só se o catálogo tiver um único elemento é que ele pode repetir).
        /// Defesa = faixa +- variância; estoque proporcional à defesa (mina mais difícil, mais rica).
        /// </summary>
        public static DepositRoll Roll(GameRules rules, DepositTierOptions tier, int playerId, DateOnly week, int rerollCount, Element? exclude)
        {
            // semente fixa e inteira (nunca HashCode.Combine/GetHashCode: variam a cada execução)
            var random = new Random(unchecked(playerId * 7919 + week.DayNumber * 104729 + tier.Slot * 1299709 + rerollCount * 15485863));

            List<Element> elements = rules.Balance.Ores
                .Where(o => o.Kind == OreKind.Gem)
                .Select(o => o.Element)
                .Distinct()
                .OrderBy(e => (int)e)
                .ToList();

            if (exclude is { } current && elements.Count > 1) elements.Remove(current);

            Element element = elements[random.Next(elements.Count)];

            double factor = 1 + tier.DefenseVariance * (2 * random.NextDouble() - 1);
            int defense = Math.Max(1, (int)Math.Round(tier.Defense * factor, MidpointRounding.AwayFromZero));
            int stock = Math.Max(1, (int)Math.Round(defense * rules.Balance.Deposits.StockPerDefense, MidpointRounding.AwayFromZero));

            return new DepositRoll(element, defense, stock);
        }
    }
}
