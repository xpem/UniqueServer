using BaseModels;
using Microsoft.Extensions.Logging;
using MobModels;
using MobRepo;
using MobService.Interfaces;

namespace MobService
{
    public class PetService(IPetRepo petRepo, IPetActionRepo actionRepo, ILogger<PetService> logger) : IPetService
    {
        public async Task<BaseResp> SavePetAsync(int userId, Pet pet)
        {
            try
            {
                logger.LogInformation("SavePetAsync: UserId={UserId}, Phase={Phase}", userId, pet.Phase);
                
                // Verifica se já existe um pet ativo para o usuário
                var existingPet = await petRepo.GetActiveByUserIdAsync(userId);

                if (existingPet != null)
                {
                    logger.LogInformation("Updating existing pet: PetId={PetId}", existingPet.Id);
                    
                    // Atualiza o pet existente
                    existingPet.Phase = pet.Phase;
                    existingPet.PhaseStart = pet.PhaseStart;
                    existingPet.Hunger = pet.Hunger;
                    existingPet.Energy = pet.Energy;
                    existingPet.Health = pet.Health;
                    existingPet.Joy = pet.Joy;
                    existingPet.PoopCount = pet.PoopCount;
                    existingPet.BowlPortions = pet.BowlPortions;
                    existingPet.Toys = pet.Toys;
                    existingPet.IsDead = pet.IsDead;
                    existingPet.DeathCause = pet.DeathCause;
                    existingPet.Name = pet.Name;

                    await petRepo.UpdateAsync(existingPet);
                    return new BaseResp(existingPet);
                }
                else
                {
                    logger.LogInformation("Creating new pet for UserId={UserId}", userId);
                    
                    // Cria um novo pet
                    pet.UserId = userId;
                    pet.CreatedAt = DateTime.UtcNow;
                    await petRepo.CreateAsync(pet);
                    return new BaseResp(pet);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SavePetAsync EXCEPTION: UserId={UserId}", userId);
                return new BaseResp(ErrorCode.ErrorCreatingObject, ex.Message);
            }
        }

        public async Task<BaseResp> GetActivePetAsync(int userId)
        {
            try
            {
                logger.LogInformation("GetActivePetAsync: UserId={UserId}", userId);
                
                var pet = await petRepo.GetActiveByUserIdAsync(userId);
                
                if (pet == null)
                {
                    logger.LogInformation("No active pet found for UserId={UserId}", userId);
                    return new BaseResp(ErrorCode.InvalidObject, "Nenhum pet encontrado. Coloque uma coleira no seu pet primeiro!");
                }

                return new BaseResp(pet);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetActivePetAsync EXCEPTION: UserId={UserId}", userId);
                return new BaseResp(ErrorCode.InvalidObject, ex.Message);
            }
        }

        public async Task<BaseResp> GetPetHistoryAsync(int userId)
        {
            try
            {
                logger.LogInformation("GetPetHistoryAsync: UserId={UserId}", userId);
                
                var pets = await petRepo.GetAllByUserIdAsync(userId);
                return new BaseResp(pets);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetPetHistoryAsync EXCEPTION: UserId={UserId}", userId);
                return new BaseResp(ErrorCode.InvalidObject, ex.Message);
            }
        }

        public async Task<BaseResp> LogActionAsync(int userId, PetActionType actionType, string? details = null)
        {
            try
            {
                logger.LogInformation("LogActionAsync: UserId={UserId}, Action={Action}", userId, actionType);
                
                // Busca o pet ativo do usuário
                var pet = await petRepo.GetActiveByUserIdAsync(userId);
                
                if (pet == null)
                {
                    return new BaseResp(ErrorCode.InvalidObject, "Nenhum pet encontrado");
                }

                var action = new PetAction
                {
                    PetId = pet.Id,
                    ActionType = actionType,
                    Details = details,
                    CreatedAt = DateTime.UtcNow
                };

                await actionRepo.CreateAsync(action);
                return new BaseResp(action);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "LogActionAsync EXCEPTION: UserId={UserId}, Action={Action}", userId, actionType);
                return new BaseResp(ErrorCode.ErrorCreatingObject, ex.Message);
            }
        }

        public async Task<BaseResp> GetActionsAsync(int userId, int limit = 50)
        {
            try
            {
                logger.LogInformation("GetActionsAsync: UserId={UserId}, Limit={Limit}", userId, limit);
                
                // Busca o pet ativo do usuário
                var pet = await petRepo.GetActiveByUserIdAsync(userId);
                
                if (pet == null)
                {
                    return new BaseResp(ErrorCode.InvalidObject, "Nenhum pet encontrado");
                }

                var actions = await actionRepo.GetByPetIdAsync(pet.Id, limit);
                return new BaseResp(actions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetActionsAsync EXCEPTION: UserId={UserId}", userId);
                return new BaseResp(ErrorCode.InvalidObject, ex.Message);
            }
        }
    }
}
