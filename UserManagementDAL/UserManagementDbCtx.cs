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
        //EntityFrameworkCore\Add-Migration "Init" -Context UserManagementDbContext
        //EntityFrameworkCore\update-database -Context UserManagementDbContext

        //to remove last migration snapshot
        //EntityFrameworkCore\Remove-Migration -Context UserManagementDbContext 
    }
}