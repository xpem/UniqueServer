using Microsoft.EntityFrameworkCore;
using MobModels;

namespace MobRepo
{
    public class MobDbCtx(DbContextOptions<MobDbCtx> options) : DbContext(options)
    {
        public DbSet<Pet> Pet => Set<Pet>();

        // migrations
        // No console do gerenciador de pacotes selecione o projeto MobService:
        // EntityFrameworkCore\Add-Migration "InitialCreate" -Context MobDbCtx
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
        }
    }
}
