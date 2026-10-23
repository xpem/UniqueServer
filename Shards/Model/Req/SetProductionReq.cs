namespace Shards.Model.Req
{
    public record SetProductionReq
    {
        /// <summary>Minério em que a mina deve se especializar (ex.: "Sapphire", "Ruby", "Emerald" ou "Quartz"). É texto para um valor inválido sair como InvalidProduction.</summary>
        public string? Focus { get; set; }
    }
}
