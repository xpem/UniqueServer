using BaseModels;
using MobModels;

namespace MobService.Interfaces
{
    public interface IPetService
    {
        /// <summary>
        /// Salva ou atualiza o pet do usuário (coloca a coleira)
        /// </summary>
        Task<BaseResp> SavePetAsync(int userId, Pet pet);

        /// <summary>
        /// Carrega o pet ativo do usuário
        /// </summary>
        Task<BaseResp> GetActivePetAsync(int userId);

        /// <summary>
        /// Lista todos os pets do usuário (histórico)
        /// </summary>
        Task<BaseResp> GetPetHistoryAsync(int userId);

        /// <summary>
        /// Registra uma ação feita no pet
        /// </summary>
        Task<BaseResp> LogActionAsync(int userId, PetActionType actionType, string? details = null);

        /// <summary>
        /// Busca histórico de ações do pet
        /// </summary>
        Task<BaseResp> GetActionsAsync(int userId, int limit = 50);
    }
}
