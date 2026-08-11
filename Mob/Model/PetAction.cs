using System.ComponentModel.DataAnnotations;

namespace MobModels
{
    /// <summary>
    /// Registro de uma ação realizada no pet
    /// </summary>
    public class PetAction : BaseModels.BaseModel
    {
        /// <summary>
        /// ID do pet que recebeu a ação
        /// </summary>
        public required int PetId { get; set; }

        /// <summary>
        /// Tipo da ação realizada
        /// </summary>
        public required PetActionType ActionType { get; set; }

        /// <summary>
        /// Detalhes adicionais (JSON opcional)
        /// </summary>
        [MaxLength(500)]
        public string? Details { get; set; }
    }

    /// <summary>
    /// Tipos de ações que podem ser feitas no pet
    /// </summary>
    public enum PetActionType
    {
        Feed = 1,       // Alimentou
        Play = 2,       // Brincou
        Sleep = 3,      // Dormiu
        Wake = 4,       // Acordou
        Medicine = 5,   // Deu remédio
        Clean = 6,      // Limpou cocô
        LightOn = 7,    // Acendeu luz
        LightOff = 8,   // Apagou luz
        Born = 9,       // Nasceu (eclosão)
        Evolved = 10,   // Evoluiu de fase
        Died = 11       // Morreu
    }
}
