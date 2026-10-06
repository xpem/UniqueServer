using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;
using Shards.Errors;
using Shards.Model.Req;
using Shards.Model.Res;
using Shards.Service;

namespace UniqueServer.Controllers
{
    [Route("[Controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting(ShardsRateLimit.PolicyName)]
    public class ShardsController(IPlayerService playerService, IMineService mineService, IPlayerSkillService playerSkillService, IPlayerMissionService playerMissionService) : BaseController
    {
        /// <summary>Estado completo do jogador. Cria o jogador (Mina 1 e Missão 1) no primeiro acesso.</summary>
        [HttpGet("state")]
        [ProducesResponseType<ShardsStateRes>(StatusCodes.Status200OK)]
        public Task<IActionResult> GetState() => RunAsync(() => playerService.GetStateAsync(Uid));

        /// <summary>Mineração ativa. O body é opcional: sem ele, 1 mineração. Com times, até o máximo configurado.</summary>
        [HttpPost("mines/{mineNumber:int}/mine")]
        [ProducesResponseType<MineRes>(StatusCodes.Status200OK)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status409Conflict)]
        public Task<IActionResult> Mine(int mineNumber, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] MineReq? req) =>
            RunAsync(() => mineService.MineAsync(Uid, mineNumber, req?.Times ?? 1));

        /// <summary>Esvazia o vagonete da mina, passando os minérios acumulados para o inventário.</summary>
        [HttpPost("mines/{mineNumber:int}/collect-cart")]
        [ProducesResponseType<CollectCartRes>(StatusCodes.Status200OK)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status409Conflict)]
        public Task<IActionResult> CollectCart(int mineNumber) => RunAsync(() => mineService.CollectCartAsync(Uid, mineNumber));

        /// <summary>Compra a próxima mina, debitando o custo (150 Bronze + 30 Prata para a Mina 2).</summary>
        [HttpPost("mines/unlock")]
        [ProducesResponseType<UnlockMineRes>(StatusCodes.Status200OK)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status409Conflict)]
        public Task<IActionResult> UnlockMine() => RunAsync(() => mineService.UnlockNextAsync(Uid));

        /// <summary>Gasta 1 ponto de habilidade na skill informada (ex.: "DoubleDrop").</summary>
        [HttpPost("skills/distribute")]
        [ProducesResponseType<DistributeSkillRes>(StatusCodes.Status200OK)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status409Conflict)]
        public Task<IActionResult> DistributeSkill([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] DistributeSkillReq? req) =>
            RunAsync(() => playerSkillService.DistributeAsync(Uid, req?.SkillType));

        /// <summary>Resgata a recompensa de uma missão concluída e libera a próxima.</summary>
        [HttpPost("missions/{missionType}/claim")]
        [ProducesResponseType<ClaimMissionRes>(StatusCodes.Status200OK)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status409Conflict)]
        public Task<IActionResult> ClaimMission(string missionType) => RunAsync(() => playerMissionService.ClaimAsync(Uid, missionType));

        /// <summary>
        /// Executa uma ação do jogo e traduz ShardsException para { code, message } com o status HTTP do código
        /// (400 para regra de negócio, 409 para conflito de concorrência).
        /// </summary>
        protected async Task<IActionResult> RunAsync<T>(Func<Task<T>> action)
        {
            try
            {
                return Ok(await action());
            }
            catch (ShardsException ex)
            {
                return StatusCode(ShardsErrors.StatusCodeOf(ex.Code), ex.ToResponse());
            }
        }
    }
}
