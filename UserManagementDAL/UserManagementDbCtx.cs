using Microsoft.EntityFrameworkCore;
using UserManagementModels;

namespace UserManagementRepo
{
    public class UserManagementDbCtx(DbContextOptions<UserManagementDbCtx> options) : DbContext(options)
    {
        public DbSet<User> User => Set<User>();

        public DbSet<UserHistoric> UserHistoric => Set<UserHistoric>();

        //public DbSet<UserHistoricType> UserHistoricType => Set<UserHistoricType>();


        //migrations
        //no console do gerenciador de pacotes selecione o dal referente:
        //EntityFrameworkCore\Add-Migration "202607041" -Context UserManagementDbCtx
        //EntityFrameworkCore\update-database -Context UserManagementDbCtx

        //to remove last migration snapshot
        //EntityFrameworkCore\Remove-Migration -Context UserManagementDbContext 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id).UseIdentityByDefaultColumn();
            });

            modelBuilder.Entity<UserHistoric>(entity =>
            {
                entity.Property(e => e.Id).UseIdentityByDefaultColumn();
            });
        }
    }
}