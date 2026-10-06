namespace Shards.Model.Req
{
    /// <summary>Body opcional de POST /shards/mines/{mineNumber}/mine.</summary>
    public record MineReq
    {
        /// <summary>Quantas mineirações (1 de energia cada) fazer numa só chamada. Padrão 1; o máximo vem do balanceamento.</summary>
        public int Times { get; set; } = 1;
    }
}
