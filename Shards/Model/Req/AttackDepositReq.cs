namespace Shards.Model.Req
{
    /// <summary>Uma pilha de gemas lapidadas no ataque. Minério e estágio são texto para um valor inválido sair como InvalidGem / InvalidStage.</summary>
    public record GemStackReq
    {
        /// <summary>Minério da gema (Ruby, Sapphire, Emerald ou Amethyst).</summary>
        public string? Ore { get; set; }

        /// <summary>Estágio lapidado (Polished ou Faceted). Gema bruta não ataca nem defende.</summary>
        public string? Stage { get; set; }

        public int Quantity { get; set; }
    }

    /// <summary>Body de POST /shards/deposits/{id}/attack.</summary>
    public record AttackDepositReq
    {
        /// <summary>Gemas que atacam, na ordem de uso. O ataque para quando a defesa do depósito chega a zero e só gasta as necessárias.</summary>
        public List<GemStackReq>? Attackers { get; set; }

        /// <summary>Gemas que defendem do contra-ataque (opcional), na ordem de uso. Só se gasta o que absorve.</summary>
        public List<GemStackReq>? Defenders { get; set; }

        /// <summary>Verdadeiro: calcula tudo (dano, gemas, contra-ataque, energia) sem gravar nada.</summary>
        public bool Preview { get; set; }
    }
}
