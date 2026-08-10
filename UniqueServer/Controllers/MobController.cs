using BaseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobModels;
using MobModels.Request;
using MobService.Interfaces;

namespace UniqueServer.Controllers
{
    [Route("[Controller]")]
    [ApiController]
    public class MobController(ILogger<MobController> logger, IPetService petService) : BaseController
    {
        /// <summary>
        /// Salva o estado do pet (coloca a coleira)
        /// </summary>
        [Route("Pet")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SavePet(ReqSavePet req)
        {
            try
            {
                logger.LogInformation("SavePet: UserId={UserId}, Phase={Phase}, IsDead={IsDead}", Uid, req.Phase, req.IsDead);

                var pet = new Pet
                {
                    UserId = Uid,
                    Name = req.Name,
                    VariantIndex = req.VariantIndex,
                    Phase = req.Phase,
                    PhaseStart = req.PhaseStart,
                    BornAt = req.BornAt,
                    Hunger = req.Hunger,
                    Energy = req.Energy,
                    Health = req.Health,
                    Joy = req.Joy,
                    PoopCount = req.PoopCount,
                    BowlPortions = req.BowlPortions,
                    Toys = req.Toys,
                    IsDead = req.IsDead,
                    DeathCause = req.DeathCause,
                    CreatedAt = DateTime.UtcNow
                };

                return BuildResponse(await petService.SavePetAsync(Uid, pet));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SavePet EXCEPTION: UserId={UserId}", Uid);
                return StatusCode(500, new { error = new { message = ex.Message } });
            }
        }

        /// <summary>
        /// Carrega o pet ativo do usuário
        /// </summary>
        [Route("Pet")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetPet()
        {
            try
            {
                logger.LogInformation("GetPet: UserId={UserId}", Uid);
                return BuildResponse(await petService.GetActivePetAsync(Uid));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetPet EXCEPTION: UserId={UserId}", Uid);
                return StatusCode(500, new { error = new { message = ex.Message } });
            }
        }

        /// <summary>
        /// Lista histórico de pets do usuário
        /// </summary>
        [Route("Pet/History")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetPetHistory()
        {
            try
            {
                logger.LogInformation("GetPetHistory: UserId={UserId}", Uid);
                return BuildResponse(await petService.GetPetHistoryAsync(Uid));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetPetHistory EXCEPTION: UserId={UserId}", Uid);
                return StatusCode(500, new { error = new { message = ex.Message } });
            }
        }
    }
}
