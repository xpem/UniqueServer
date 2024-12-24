using Microsoft.EntityFrameworkCore;
using UserModels;

namespace UserManagementDAL
{
    public class UserManagementDbContext : DbContext
    {
        public DbSet<User> User => Set<User>();

        public DbSet<UserHistoric> UserHistoric => Set<UserHistoric>();

        public DbSet<UserHistoricType> UserHistoricType => Set<UserHistoricType>();

        public UserManagementDbContext(DbContextOptions<UserManagementDbContext> options) : base(options)
        {
        }


        //migrations
        //no console do gerenciador de pacotes selecione o dal referente:
        //EntityFrameworkCore\Add-Migration "Init" -Context UserManagementDbContext
        //EntityFrameworkCore\update-database -Context UserManagementDbContext

        //to remove last migration snapshot
        //EntityFrameworkCore\Remove-Migration -Context UserManagementDbContext 
    }
}