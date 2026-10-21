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

        public virtual DbSet<DailyMissionDTO> DailyMission => Set<DailyMissionDTO>();

        //migrations
        //no console do gerenciador de pacotes selecione o projeto Shards:
        //EntityFrameworkCore\Add-Migration "21102026" -Context ShardsDbctx
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

                // o padrão no banco faz as minas já existentes virarem Quartz ao aplicar a migration (CLR default seria 0)
                entity.Property(e => e.ProductionType).HasDefaultValue(OreType.Quartz).HasSentinel(OreType.Quartz);

                ConfigureVersion(entity);
            });

            modelBuilder.Entity<InventoryItemDTO>(entity =>
            {
                entity.HasOne<PlayerDTO>().WithMany().HasForeignKey(e => e.PlayerId).OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.PlayerId, e.OreType, e.Stage })
                    .IsUnique()
                    .HasDatabaseName("IX_InventoryItem_PlayerId_OreType_Stage");

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

            modelBuilder.Entity<DailyMissionDTO>(entity =>
            {
                entity.HasOne<PlayerDTO>().WithMany().HasForeignKey(e => e.PlayerId).OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.PlayerId, e.Day, e.DailyType })
                    .IsUnique()
                    .HasDatabaseName("IX_DailyMission_PlayerId_Day_DailyType");

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
