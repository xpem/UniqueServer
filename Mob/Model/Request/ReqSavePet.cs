using System.ComponentModel.DataAnnotations;

namespace MobModels.Request
{
    /// <summary>
    /// Request para salvar o estado do pet
    /// </summary>
    public record ReqSavePet
    {
        /// <summary>
        /// Nome do pet (opcional)
        /// </summary>
        [MaxLength(50)]
        public string? Name { get; init; }

        /// <summary>
        /// Índice da variante/modelo do pet
        /// </summary>
        [Required]
        public required int VariantIndex { get; init; }

        /// <summary>
        /// Fase atual: 0=egg, 1=baby, 2=young, 3=adult
        /// </summary>
        [Required]
        [Range(0, 3)]
        public required int Phase { get; init; }

        /// <summary>
        /// Timestamp de quando a fase atual começou
        /// </summary>
        [Required]
        public required long PhaseStart { get; init; }

        /// <summary>
        /// Timestamp de nascimento do pet
        /// </summary>
        [Required]
        public required long BornAt { get; init; }

        /// <summary>
        /// Nível de fome (0-5)
        /// </summary>
        [Required]
        [Range(0, 5)]
        public required int Hunger { get; init; }

        /// <summary>
        /// Nível de energia (0-5)
        /// </summary>
        [Required]
        [Range(0, 5)]
        public required int Energy { get; init; }

        /// <summary>
        /// Nível de saúde (0-5)
        /// </summary>
        [Required]
        [Range(0, 5)]
        public required int Health { get; init; }

        /// <summary>
        /// Nível de felicidade/alegria (0-5)
        /// </summary>
        [Required]
        [Range(0, 5)]
        public required int Joy { get; init; }

        /// <summary>
        /// Quantidade de cocôs na tela (0-3)
        /// </summary>
        [Range(0, 3)]
        public int PoopCount { get; init; }

        /// <summary>
        /// Porções de comida no pote (0-3)
        /// </summary>
        [Range(0, 3)]
        public int BowlPortions { get; init; }

        /// <summary>
        /// Brinquedos disponíveis (0-1)
        /// </summary>
        [Range(0, 1)]
        public int Toys { get; init; }

        /// <summary>
        /// Pet está morto?
        /// </summary>
        public bool IsDead { get; init; }

        /// <summary>
        /// Causa da morte (old_age, neglect)
        /// </summary>
        [MaxLength(20)]
        public string? DeathCause { get; init; }
    }
}
