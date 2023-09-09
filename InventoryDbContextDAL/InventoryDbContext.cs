using Microsoft.EntityFrameworkCore;
using BaseModels;
using InventoryModels;

namespace InventoryDbContextDAL
{
    public class InventoryDbContext : DbContext
    {
        public virtual DbSet<Category> Category { get; set; }

        public virtual DbSet<SubCategory> SubCategory { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(PrivateKeys.InventoryConn, ServerVersion.AutoDetect(PrivateKeys.InventoryConn));
        }
    }
}