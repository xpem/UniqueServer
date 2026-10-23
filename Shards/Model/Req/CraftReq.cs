namespace Shards.Model.Req
{
    /// <summary>Body de POST /shards/workshop/craft.</summary>
    public record CraftReq
    {
        /// <summary>Gema a lapidar (Ruby, Sapphire ou Emerald). É texto para um valor inválido sair como InvalidGem.</summary>
        public string? Gem { get; set; }

        /// <summary>Estágio a produzir (Polished ou Faceted). É texto para um valor inválido sair como InvalidStage.</summary>
        public string? Stage { get; set; }

        /// <summary>Quantas gemas produzir numa só chamada. Padrão 1; o máximo é o mesmo de Energy.MaxMinesPerRequest.</summary>
        public int Times { get; set; } = 1;
    }
}
