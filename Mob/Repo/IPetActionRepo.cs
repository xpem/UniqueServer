using MobModels;

namespace MobRepo
{
    public interface IPetActionRepo
    {
        /// <summary>
        /// Registra uma nova ação no pet
        /// </summary>
        Task<int> CreateAsync(PetAction action);

        /// <summary>
        /// Busca ações de um pet (com paginação)
        /// </summary>
        Task<List<PetAction>> GetByPetIdAsync(int petId, int limit = 50);

        /// <summary>
        /// Conta total de ações de um pet
        /// </summary>
        Task<int> CountByPetIdAsync(int petId);

        /// <summary>
        /// Busca ações de um pet por tipo
        /// </summary>
        Task<List<PetAction>> GetByPetIdAndTypeAsync(int petId, PetActionType actionType, int limit = 20);
    }
}
