using MobModels;

namespace MobRepo
{
    public interface IPetRepo
    {
        /// <summary>
        /// Cria um novo pet para o usuário
        /// </summary>
        Task<int> CreateAsync(Pet pet);

        /// <summary>
        /// Atualiza o estado do pet
        /// </summary>
        Task<int> UpdateAsync(Pet pet);

        /// <summary>
        /// Busca o pet ativo (vivo) do usuário
        /// </summary>
        Task<Pet?> GetActiveByUserIdAsync(int userId);

        /// <summary>
        /// Busca um pet por ID
        /// </summary>
        Task<Pet?> GetByIdAsync(int id);

        /// <summary>
        /// Busca todos os pets do usuário (incluindo mortos)
        /// </summary>
        Task<List<Pet>> GetAllByUserIdAsync(int userId);

        /// <summary>
        /// Deleta um pet
        /// </summary>
        Task DeleteAsync(int id);
    }
}
