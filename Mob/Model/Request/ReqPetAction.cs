using System.ComponentModel.DataAnnotations;

namespace MobModels.Request
{
    /// <summary>
    /// Request para registrar uma ação no pet
    /// </summary>
    public record ReqPetAction
    {
        /// <summary>
        /// Tipo da ação (1=Feed, 2=Play, 3=Sleep, etc.)
        /// </summary>
        [Required]
        public required int ActionType { get; init; }

        /// <summary>
        /// Detalhes adicionais (opcional)
        /// </summary>
        [MaxLength(500)]
        public string? Details { get; init; }
    }
}
