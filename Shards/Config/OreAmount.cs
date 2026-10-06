using Shards.Model.DTO;

namespace Shards.Config
{
    /// <summary>Quantidade de um minério, usada em custos de desbloqueio e recompensas.</summary>
    public class OreAmount
    {
        public OreType Ore { get; set; }

        public int Quantity { get; set; }
    }
}
