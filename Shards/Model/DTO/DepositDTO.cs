using System.ComponentModel.DataAnnotations.Schema;

namespace Shards.Model.DTO
{
    /// <summary>
    /// Um depósito (mina finita) oferecido a um jogador numa semana. Único por (jogador, semana, slot): o slot é a faixa de dificuldade.
    /// A troca (rotate) reescreve a mesma linha. Um depósito atacado de semana passada continua valendo até se esgotar.
    /// </summary>
    [Table("Deposit")]
    public class DepositDTO
    {
        public int Id { get; set; }

        public required int PlayerId { get; set; }

        /// <summary>Faixa de dificuldade (1 = leve, 2 = média, 3 = difícil), de Deposits.Tiers.</summary>
        public required int Slot { get; set; }

        /// <summary>Segunda-feira da semana do jogo em que a oferta nasceu (a semana vira em Daily.ResetHourUtc).</summary>
        public required DateOnly WeekStart { get; set; }

        public required Element Element { get; set; }

        public required int DefenseMax { get; set; }

        public required int DefenseCurrent { get; set; }

        public required int StockMax { get; set; }

        public required int StockRemaining { get; set; }

        public DepositStatus Status { get; set; } = DepositStatus.Sealed;

        /// <summary>Quantas vezes esta oferta foi trocada. A soma da semana define o custo da próxima troca.</summary>
        public int RerollCount { get; set; }

        public required DateTime CreatedAt { get; set; }

        public uint Version { get; set; }
    }
}
