using System.ComponentModel.DataAnnotations.Schema;

namespace Shards.Model.DTO
{
    [Table("PlayerSkill")]
    public class PlayerSkillDTO
    {
        public int Id { get; set; }

        public required int PlayerId { get; set; }

        public required SkillType SkillType { get; set; }

        public int Level { get; set; }

        public uint Version { get; set; }
    }
}
