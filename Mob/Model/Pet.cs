using System.ComponentModel.DataAnnotations;

namespace MobModels
{
    /// <summary>
    /// Representa um pet virtual salvo na nuvem (com "coleira")
    /// </summary>
    public class Pet : BaseModels.BaseModel
    {
        /// <summary>
        /// ID do usuário dono do pet
        /// </summary>
        public required int UserId { get; set; }

        /// <summary>
        /// Nome do pet (opcional, dado pelo usuário)
        /// </summary>
        [MaxLength(50)]
        public string? Name { get; set; }

        /// <summary>
        /// Índice da variante/modelo do pet (Azul, Dino, Fantasma, etc.)
        /// </summary>
        public required int VariantIndex { get; set; }

        /// <summary>
        /// Fase atual: 0=egg, 1=baby, 2=young, 3=adult
        /// </summary>
        public required int Phase { get; set; }

        /// <summary>
        /// Timestamp de quando a fase atual começou
        /// </summary>
        public required long PhaseStart { get; set; }

        /// <summary>
        /// Timestamp de nascimento do pet
        /// </summary>
        public required long BornAt { get; set; }

        /// <summary>
        /// Nível de fome (0-5)
        /// </summary>
        public required int Hunger { get; set; }

        /// <summary>
        /// Nível de energia (0-5)
        /// </summary>
        public required int Energy { get; set; }

        /// <summary>
        /// Nível de saúde (0-5)
        /// </summary>
        public required int Health { get; set; }

        /// <summary>
        /// Nível de felicidade/alegria (0-5)
        /// </summary>
        public required int Joy { get; set; }

        /// <summary>
        /// Nível de doença (0-5)
        /// </summary>
        public int Sickness { get; set; }

        /// <summary>
        /// Timer acumulado para aumentar nível de doença (ms)
        /// </summary>
        public long SicknessTimer { get; set; }

        /// <summary>
        /// Quantidade de cocôs na tela (0-3)
        /// </summary>
        public int PoopCount { get; set; }

        /// <summary>
        /// Porções de comida no pote (0-3)
        /// </summary>
        public int BowlPortions { get; set; }

        /// <summary>
        /// Brinquedos disponíveis (0-1)
        /// </summary>
        public int Toys { get; set; }

        /// <summary>
        /// Pet está morto?
        /// </summary>
        public bool IsDead { get; set; }

        /// <summary>
        /// Causa da morte (old_age, neglect, null se vivo)
        /// </summary>
        [MaxLength(20)]
        public string? DeathCause { get; set; }

        /// <summary>
        /// Última atualização do estado
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
