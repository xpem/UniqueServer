using Microsoft.EntityFrameworkCore;
using Shards.Model.DTO;

namespace Shards.Repo
{
    public class ShardsDbctx(DbContextOptions<ShardsDbctx> options) : DbContext(options)
    {
        public virtual DbSet<PlayerDTO> Player => Set<PlayerDTO>();

        public virtual DbSet<PlayerSkillDTO> PlayerSkill => Set<PlayerSkillDTO>();

        public virtual DbSet<MineDTO> Mine => Set<MineDTO>();

        public virtual DbSet<InventoryItemDTO> InventoryItem => Set<InventoryItemDTO>();

        public virtual DbSet<PlayerMissionDTO> PlayerMission => Set<PlayerMissionDTO>();

        //migrations
        //no console do gerenciador de pacotes selecione o projeto Shards:
        //EntityFrameworkCore\Add-Migration "Init" -Context ShardsDbctx
        //EntityFrameworkCore\update-database -Context ShardsDbctx

        //to remove last migration snapshot
        //EntityFrameworkCore\Remove-Migration -Context ShardsDbctx

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.UseIdentityByDefaultColumns();

            modelBuilder.Entity<PlayerDTO>(entity =>
            {
                entity.HasIndex(e => e.UserId)
                    .IsUnique()
                    .HasDatabaseName("IX_Player_UserId");

                ConfigureVersion(entity);
            });

            modelBuilder.Entity<PlayerSkillDTO>(entity =>
            {
                entity.HasOne<PlayerDTO>().WithMany().HasForeignKey(e => e.PlayerId).OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.PlayerId, e.SkillType })
                    .IsUnique()
                    .HasDatabaseName("IX_PlayerSkill_PlayerId_SkillType");

                ConfigureVersion(entity);
            });

            modelBuilder.Entity<MineDTO>(entity =>
            {
                entity.HasOne<PlayerDTO>().WithMany().HasForeignKey(e => e.PlayerId).OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.PlayerId, e.MineNumber })
                    .IsUnique()
                    .HasDatabaseName("IX_Mine_PlayerId_MineNumber");

                ConfigureVersion(entity);
            });

            modelBuilder.Entity<InventoryItemDTO>(entity =>
            {
                entity.HasOne<PlayerDTO>().WithMany().HasForeignKey(e => e.PlayerId).OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.PlayerId, e.OreType })
                    .IsUnique()
                    .HasDatabaseName("IX_InventoryItem_PlayerId_OreType");

                ConfigureVersion(entity);
            });

            modelBuilder.Entity<PlayerMissionDTO>(entity =>
            {
                entity.HasOne<PlayerDTO>().WithMany().HasForeignKey(e => e.PlayerId).OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.PlayerId, e.MissionType })
                    .IsUnique()
                    .HasDatabaseName("IX_PlayerMission_PlayerId_MissionType");

                ConfigureVersion(entity);
            });
        }

        // token de concorrência otimista: coluna de sistema xmin do Postgres (não gera coluna na migration)
        private static void ConfigureVersion<T>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<T> entity) where T : class
        {
            entity.Property<uint>("Version")
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        }
    }
}
