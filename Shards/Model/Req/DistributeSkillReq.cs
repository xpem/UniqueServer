namespace Shards.Model.Req
{
    public record DistributeSkillReq
    {
        /// <summary>Nome da habilidade (ex.: "DoubleDrop"). É texto para um valor inválido sair como InvalidSkill.</summary>
        public string? SkillType { get; set; }
    }
}
