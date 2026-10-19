using Shards.Model.DTO;

namespace Shards.Errors
{
    public static class ShardsErrors
    {
        /// <summary>Status HTTP de cada código: conflito de concorrência é 409, o resto é erro de regra (400).</summary>
        public static int StatusCodeOf(ShardsErrorCode code) => code switch
        {
            ShardsErrorCode.ConcurrencyConflict => 409,
            _ => 400,
        };

        public static string DefaultMessage(ShardsErrorCode code) => code switch
        {
            ShardsErrorCode.PlayerNotFound => "Jogador não encontrado.",
            ShardsErrorCode.MineNotFound => "Mina não encontrada.",
            ShardsErrorCode.NotEnoughEnergy => "Energia insuficiente.",
            ShardsErrorCode.CartEmpty => "O vagonete está vazio.",
            ShardsErrorCode.NoSkillPoints => "Sem pontos de habilidade disponíveis.",
            ShardsErrorCode.SkillMaxLevel => "A habilidade já está no nível máximo.",
            ShardsErrorCode.InvalidSkill => "Habilidade inválida.",
            ShardsErrorCode.NotEnoughResources => "Recursos insuficientes.",
            ShardsErrorCode.NoMoreMines => "Não há mais minas para desbloquear.",
            ShardsErrorCode.MissionNotCompleted => "A missão ainda não foi concluída.",
            ShardsErrorCode.MissionNotFound => "Missão não encontrada.",
            ShardsErrorCode.MissionAlreadyClaimed => "A recompensa desta missão já foi resgatada.",
            ShardsErrorCode.InvalidProduction => "Tipo de produção inválido.",
            ShardsErrorCode.SameProduction => "A mina já está produzindo este minério.",
            ShardsErrorCode.MineSlotsFull => "Sem espaço para uma nova mina. Distribua um ponto de habilidade em MineSlots.",
            ShardsErrorCode.CartUpgradeLocked => "A habilidade CartCapacity ainda não permite este nível do vagonete.",
            ShardsErrorCode.CartMaxLevel => "O vagonete desta mina já está no nível máximo.",
            ShardsErrorCode.InvalidGem => "Gema inválida. A oficina lapida Ruby, Sapphire ou Emerald.",
            ShardsErrorCode.InvalidStage => "Estágio inválido. A oficina produz Polished ou Faceted.",
            ShardsErrorCode.WorkshopLocked => "A habilidade Workshop ainda não permite este estágio.",
            ShardsErrorCode.InvalidTimes => "Quantidade de ações inválida.",
            ShardsErrorCode.ConcurrencyConflict => "A operação conflitou com outra em andamento. Tente novamente.",
            _ => "Erro de regra do jogo.",
        };

        /// <summary>
        /// Entradas que podem vir inválidas (skill no corpo, missão na rota) chegam como texto e são convertidas aqui,
        /// para o erro sair no formato { code, message } e não no ProblemDetails automático do ASP.NET.
        /// Aceita só o nome do valor (sem número) e ignora maiúsculas.
        /// </summary>
        public static SkillType ParseSkillType(string? value) =>
            TryParseName(value, out SkillType result) ? result : throw new ShardsException(ShardsErrorCode.InvalidSkill);

        public static MissionType ParseMissionType(string? value) =>
            TryParseName(value, out MissionType result) ? result : throw new ShardsException(ShardsErrorCode.MissionNotFound);

        public static OreType ParseProduction(string? value) =>
            TryParseName(value, out OreType result) ? result : throw new ShardsException(ShardsErrorCode.InvalidProduction);

        /// <summary>Gema da oficina: qualquer minério menos o quartzo (moeda, sem estágios).</summary>
        public static OreType ParseGem(string? value) =>
            TryParseName(value, out OreType result) && result != OreType.Quartz ? result : throw new ShardsException(ShardsErrorCode.InvalidGem);

        /// <summary>Estágio que a oficina produz: Polished ou Faceted (Raw não se fabrica).</summary>
        public static ItemStage ParseCraftStage(string? value) =>
            TryParseName(value, out ItemStage result) && result != ItemStage.Raw ? result : throw new ShardsException(ShardsErrorCode.InvalidStage);

        private static bool TryParseName<T>(string? value, out T result) where T : struct, Enum
        {
            result = default;

            // só letras: Enum.TryParse também aceita número ("1") e lista com vírgula ("A,B", que vira o OR dos valores)
            string name = value?.Trim() ?? "";
            if (name.Length == 0 || !name.All(char.IsLetter)) return false;

            return Enum.TryParse(name, ignoreCase: true, out result) && Enum.IsDefined(result);
        }
    }
}
