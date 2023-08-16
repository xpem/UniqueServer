using BaseModels;
using Microsoft.EntityFrameworkCore;
using UserModels;

namespace DALDbContext
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> User { get; set; }

        public DbSet<UserHistoric> UserHistoric { get; set; }

        public DbSet<UserHistoricType> UserHistoricType { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(PrivateKeys.UserConn, ServerVersion.AutoDetect(PrivateKeys.UserConn));
            // b => b.MigrationsAssembly("UserManagementServer")
        }
    }
}