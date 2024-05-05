using BaseModels;
using InventoryModels;
using Microsoft.EntityFrameworkCore;

namespace InventoryDbContextDAL
{
    public class InventoryDbContext : DbContext
    {
        public virtual DbSet<Category> Category { get; set; }

        public virtual DbSet<SubCategory> SubCategory { get; set; }

        public virtual DbSet<ItemSituation> ItemSituation { get; set; }

        public virtual DbSet<AcquisitionType> AcquisitionType { get; set; }

        public virtual DbSet<Item> Item { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(PrivateKeys.InventoryConn, ServerVersion.AutoDetect(PrivateKeys.InventoryConn));
        }

        //migrations
        //no console do gerenciador de pacotes selecione o dal referente:
        //Add-Migration "Init" -Context InventoryDbContext
        //update-database -Context InventoryDbContext

        //to remove last migration snapshot
        //Remove-Migration -Context InventoryDbContext 
    }
}