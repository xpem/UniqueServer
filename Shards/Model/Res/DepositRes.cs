using Shards.Model.DTO;

namespace Shards.Model.Res
{
    /// <summary>Um depósito oferecido ao jogador: elemento, defesa, estoque e o que ele rende.</summary>
    public record DepositRes
    {
        public int Id { get; set; }

        /// <summary>Faixa de dificuldade (1 = leve, 2 = média, 3 = difícil).</summary>
        public int Slot { get; set; }

        /// <summary>Nome da faixa (Light, Medium, Hard).</summary>
        public required string Tier { get; set; }

        public Element Element { get; set; }

        public int DefenseCurrent { get; set; }

        public int DefenseMax { get; set; }

        public int StockRemaining { get; set; }

        public int StockMax { get; set; }

        /// <summary>Situação para o jogador (um intocado de semana passada já vem como Expired).</summary>
        public DepositStatus Status { get; set; }

        /// <summary>A oferta ainda está intocada e dentro da semana: dá para trocá-la (rotate).</summary>
        public bool CanRotate { get; set; }

        /// <summary>Dá para atacar: ainda tem defesa, não expirou nem se esgotou e, se estiver intocado, ainda cabe mais um depósito aberto.</summary>
        public bool CanAttack { get; set; }

        /// <summary>Dá para extrair: a defesa já foi derrubada (zero), o depósito está aberto e ainda tem estoque.</summary>
        public bool CanMine { get; set; }

        /// <summary>Contra-ataque por ponto de dano causado (0,05 = 5%). O que as gemas de defesa não absorvem tira energia de exploração.</summary>
        public double CounterRatio { get; set; }

        /// <summary>Quando a oferta expira (a virada da semana); nulo se já foi atacada (persiste até se esgotar).</summary>
        public DateTime? ExpiresAtUtc { get; set; }

        /// <summary>O que a extração rende, com as probabilidades já normalizadas.</summary>
        public List<DropRes> Drops { get; set; } = [];
    }

    /// <summary>Trocas da semana: as primeiras são grátis; depois cada uma custa mais que a anterior.</summary>
    public record DepositRerollsRes
    {
        /// <summary>Trocas já feitas nesta semana, somando todos os slots.</summary>
        public int Used { get; set; }

        public int FreeLeft { get; set; }

        /// <summary>Custo da próxima troca; nulo quando ainda há troca grátis.</summary>
        public OreAmountRes? NextCost { get; set; }

        /// <summary>A próxima troca está ao alcance do inventário (sempre verdadeiro enquanto é grátis).</summary>
        public bool CanAfford { get; set; }
    }

    /// <summary>Bloco de depósitos do state. Nulo até o jogador liberar a oficina (skill Workshop).</summary>
    public record DepositsStateRes
    {
        /// <summary>Segunda-feira da semana do jogo (yyyy-MM-dd).</summary>
        public required string WeekStart { get; set; }

        /// <summary>Quando a semana vira: as ofertas intocadas expiram e nascem as novas.</summary>
        public DateTime ResetsAtUtc { get; set; }

        /// <summary>Depósitos já atacados e ainda não esgotados.</summary>
        public int OpenCount { get; set; }

        /// <summary>Limite de depósitos abertos ao mesmo tempo (quem recusa é o ataque).</summary>
        public int MaxOpen { get; set; }

        /// <summary>Energia de exploração gasta por unidade extraída.</summary>
        public int EnergyPerExtraction { get; set; }

        public required DepositRerollsRes Rerolls { get; set; }

        /// <summary>As ofertas da semana (por slot) e os depósitos atacados de semanas anteriores que ainda persistem.</summary>
        public List<DepositRes> Items { get; set; } = [];
    }

    /// <summary>Resposta de POST /shards/deposits/{id}/rotate.</summary>
    public record RotateDepositRes
    {
        /// <summary>A oferta nova (no mesmo slot).</summary>
        public required DepositRes Deposit { get; set; }

        /// <summary>O que a troca custou; nulo quando foi grátis.</summary>
        public OreAmountRes? Spent { get; set; }

        public List<OreAmountRes> Inventory { get; set; } = [];

        /// <summary>O bloco de depósitos completo, já atualizado (contagem de trocas e custo da próxima).</summary>
        public required DepositsStateRes Deposits { get; set; }
    }

    /// <summary>Uma pilha de gemas lapidadas usada (ou a usar, na prévia) num ataque.</summary>
    public record GemStackRes
    {
        public OreType OreType { get; set; }

        public ItemStage Stage { get; set; }

        public int Quantity { get; set; }
    }

    /// <summary>
    /// Resposta de POST /shards/deposits/{id}/attack. Com preview, nada foi gravado e os campos descrevem o que aconteceria.
    /// </summary>
    public record AttackDepositRes
    {
        /// <summary>Verdadeiro se foi só a prévia (nada foi gravado).</summary>
        public bool Preview { get; set; }

        /// <summary>Na prévia: o ataque real seria aceito agora. No ataque real, sempre verdadeiro.</summary>
        public bool CanConfirm { get; set; }

        /// <summary>Na prévia, quando CanConfirm é falso: o código de erro que o ataque real devolveria (NotEnoughResources, NotEnoughExploreEnergy, TooManyOpenDeposits).</summary>
        public string? Blocker { get; set; }

        public int DefenseBefore { get; set; }

        /// <summary>Dano efetivamente aplicado (nunca passa da defesa que restava).</summary>
        public int DamageDealt { get; set; }

        public int DefenseAfter { get; set; }

        /// <summary>A defesa chegou a zero: o depósito está pronto para a extração (fatia seguinte).</summary>
        public bool Broken { get; set; }

        /// <summary>Gemas atacantes gastas (só as necessárias para zerar ou para o dano feito).</summary>
        public List<GemStackRes> AttackersUsed { get; set; } = [];

        /// <summary>Gemas defensoras gastas (só o que foi preciso para absorver o contra-ataque).</summary>
        public List<GemStackRes> DefendersUsed { get; set; } = [];

        /// <summary>Contra-ataque do depósito em pontos (dano x razão da faixa).</summary>
        public double CounterAttack { get; set; }

        public double Absorbed { get; set; }

        public double Unabsorbed { get; set; }

        /// <summary>Energia de exploração perdida (o que não foi absorvido x conversão, arredondado para cima).</summary>
        public int EnergyCost { get; set; }

        /// <summary>Energia de exploração que sobra depois do ataque (na prévia, a que sobraria).</summary>
        public int ExploreEnergyAfter { get; set; }

        /// <summary>O depósito depois do ataque (na prévia, como está agora).</summary>
        public required DepositRes Deposit { get; set; }

        /// <summary>Inventário completo (todos os estágios) depois do ataque.</summary>
        public required InventoryRes Inventory { get; set; }

        public required PlayerRes Player { get; set; }

        /// <summary>O bloco de depósitos completo, atualizado.</summary>
        public required DepositsStateRes Deposits { get; set; }

        /// <summary>Missões que mudaram nesta ação (só no ataque real).</summary>
        public List<MissionRes> Missions { get; set; } = [];
    }

    /// <summary>Resposta de POST /shards/deposits/{id}/mine.</summary>
    public record MineDepositRes
    {
        /// <summary>Quantas unidades foram extraídas (menos que o pedido se o estoque restante era menor).</summary>
        public int Units { get; set; }

        /// <summary>O que rendeu, por minério.</summary>
        public List<OreAmountRes> Drops { get; set; } = [];

        public int XpGained { get; set; }

        public bool LevelUp { get; set; }

        /// <summary>Energia de exploração gasta.</summary>
        public int EnergyCost { get; set; }

        /// <summary>O estoque acabou nesta extração: o depósito passou a Depleted e liberou uma vaga de depósito aberto.</summary>
        public bool Depleted { get; set; }

        public required DepositRes Deposit { get; set; }

        /// <summary>Minério bruto do jogador depois da extração (mesmo formato de state.inventory).</summary>
        public List<OreAmountRes> Inventory { get; set; } = [];

        public required PlayerRes Player { get; set; }

        /// <summary>O bloco de depósitos completo, atualizado.</summary>
        public required DepositsStateRes Deposits { get; set; }

        /// <summary>Missões que mudaram nesta ação.</summary>
        public List<MissionRes> Missions { get; set; } = [];
    }
}
