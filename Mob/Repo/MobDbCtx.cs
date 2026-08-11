using Microsoft.EntityFrameworkCore;
using MobModels;

namespace MobRepo
{
    public class MobDbCtx(DbContextOptions<MobDbCtx> options) : DbContext(options)
    {
        public DbSet<Pet> Pet => Set<Pet>();
        public DbSet<PetAction> PetAction => Set<PetAction>();

        // migrations
        // No console do gerenciador de pacotes selecione o projeto MobService:
        // EntityFrameworkCore\Add-Migration "100820261" -Context MobDbCtx
        // EntityFrameworkCore\update-database -Context MobDbCtx

        // To remove last migration snapshot:
        // EntityFrameworkCore\Remove-Migration -Context MobDbCtx

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Pet>(entity =>
            {
                entity.Property(e => e.Id).UseIdentityByDefaultColumn();
                
                // Índice para buscar pets por usuário
                entity.HasIndex(e => e.UserId);
                
                // Um usuário pode ter apenas um pet ativo (não morto) por vez
                entity.HasIndex(e => new { e.UserId, e.IsDead });
            });

            modelBuilder.Entity<PetAction>(entity =>
            {
                entity.Property(e => e.Id).UseIdentityByDefaultColumn();
                
                // Índice para buscar ações por pet
                entity.HasIndex(e => e.PetId);
                
                // Índice para buscar ações por data
                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
}
