using BaseModels;
using Microsoft.EntityFrameworkCore;
using UserModels;

namespace UserManagementDAL
{
    public class UserManagementDbContext : DbContext
    {
        public DbSet<User> User { get; set; }

        public DbSet<UserHistoric> UserHistoric { get; set; }

        public DbSet<UserHistoricType> UserHistoricType { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(PrivateKeys.UserManagementConn, ServerVersion.AutoDetect(PrivateKeys.UserManagementConn));
            // b => b.MigrationsAssembly("UserManagementServer")
        }
    }
}