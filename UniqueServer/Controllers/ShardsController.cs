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
    public class ShardsController(IPlayerService playerService, IMineService mineService, IPlayerSkillService playerSkillService, IPlayerMissionService playerMissionService, IInventoryService inventoryService, IWorkshopService workshopService, IDailyService dailyService) : BaseController
    {
        /// <summary>Resgata o baú de hoje (exige todas as missões diárias resgatadas) e atualiza a sequência de dias.</summary>
        [HttpPost("daily/chest/claim")]
        [ProducesResponseType<DailyClaimRes>(StatusCodes.Status200OK)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status409Conflict)]
        public Task<IActionResult> ClaimDailyChest() => RunAsync(() => dailyService.ClaimChestAsync(Uid));

        /// <summary>Resgata a recompensa de uma missão diária concluída hoje (ex.: Energy).</summary>
        [HttpPost("daily/{type}/claim")]
        [ProducesResponseType<DailyClaimRes>(StatusCodes.Status200OK)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status409Conflict)]
        public Task<IActionResult> ClaimDaily(string type) => RunAsync(() => dailyService.ClaimAsync(Uid, type));

        /// <summary>
        /// Oficina de Lapidação: gasta gemas do estágio anterior e minério bruto e produz gemas Polished ou Faceted
        /// (instantâneo, sem risco). Exige a skill Workshop (nível 1 = Polished, nível 2 = Faceted).
        /// </summary>
        [HttpPost("workshop/craft")]
        [ProducesResponseType<CraftRes>(StatusCodes.Status200OK)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status409Conflict)]
        public Task<IActionResult> Craft([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CraftReq? req) =>
            RunAsync(() => workshopService.CraftAsync(Uid, req?.Gem, req?.Stage, req?.Times ?? 1));

        /// <summary>Inventário completo por minério e estágio (bruto, polido, facetado); só itens com quantidade maior que zero.</summary>
        [HttpGet("inventory")]
        [ProducesResponseType<InventoryRes>(StatusCodes.Status200OK)]
        public Task<IActionResult> GetInventory() => RunAsync(() => inventoryService.GetAsync(Uid));

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

        /// <summary>
        /// Muda o tipo de produção da mina (ex.: Sapphire), pagando o custo em minério. Voltar para Quartz é de graça.
        /// O vagonete é esvaziado antes, com a produção antiga.
        /// </summary>
        [HttpPost("mines/{mineNumber:int}/production")]
        [ProducesResponseType<SetProductionRes>(StatusCodes.Status200OK)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status409Conflict)]
        public Task<IActionResult> SetProduction(int mineNumber, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SetProductionReq? req) =>
            RunAsync(() => mineService.SetProductionAsync(Uid, mineNumber, req?.Focus));

        /// <summary>
        /// Sobe o vagonete da mina para o próximo nível, pagando o custo em minério. Exige a skill CartCapacity no nível (teto).
        /// O vagonete é esvaziado antes.
        /// </summary>
        [HttpPost("mines/{mineNumber:int}/upgrade-cart")]
        [ProducesResponseType<UpgradeCartRes>(StatusCodes.Status200OK)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ShardsErrorRes>(StatusCodes.Status409Conflict)]
        public Task<IActionResult> UpgradeCart(int mineNumber) => RunAsync(() => mineService.UpgradeCartAsync(Uid, mineNumber));

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
