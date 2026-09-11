using InventoryModels.DTOs;
using Microsoft.EntityFrameworkCore;

namespace InventoryRepos
{
    public class InventoryDbCtx(DbContextOptions<InventoryDbCtx> options) : DbContext(options)
    {
        public virtual DbSet<Category> Category => Set<Category>();

        public virtual DbSet<SubCategory> SubCategory => Set<SubCategory>();

        public virtual DbSet<ItemSituation> ItemSituation => Set<ItemSituation>();

        public virtual DbSet<AcquisitionType> AcquisitionType => Set<AcquisitionType>();

        public virtual DbSet<Item> Item => Set<Item>();

        public virtual DbSet<ItemHistoric> ItemHistoric => Set<ItemHistoric>();

        public virtual DbSet<ItemHistoricItem> ItemHistoricItem => Set<ItemHistoricItem>();

        public virtual DbSet<ItemHistoricType> ItemHistoricType => Set<ItemHistoricType>();

        public virtual DbSet<ItemHistoricItemField> ItemHistoricItemField => Set<ItemHistoricItemField>();

        public virtual DbSet<CategoryHistoric> CategoryHistoric => Set<CategoryHistoric>();

        public virtual DbSet<CategoryHistoricItem> CategoryHistoricItem => Set<CategoryHistoricItem>();

        public virtual DbSet<CategoryHistoricType> CategoryHistoricType => Set<CategoryHistoricType>();

        public virtual DbSet<CategoryHistoricItemField> CategoryHistoricItemField => Set<CategoryHistoricItemField>();

        public virtual DbSet<SubCategoryHistoric> SubCategoryHistoric => Set<SubCategoryHistoric>();

        public virtual DbSet<SubCategoryHistoricItem> SubCategoryHistoricItem => Set<SubCategoryHistoricItem>();

        public virtual DbSet<SubCategoryHistoricType> SubCategoryHistoricType => Set<SubCategoryHistoricType>();

        public virtual DbSet<SubCategoryHistoricItemField> SubCategoryHistoricItemField => Set<SubCategoryHistoricItemField>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseIdentityByDefaultColumns();

            modelBuilder.Entity<Item>(b =>
            {
                b.HasIndex(x => new { x.UserId, x.CreatedAt })
                    .HasDatabaseName("IX_Item_UserId_CreatedAt");

                b.HasIndex(x => new { x.UserId, x.ItemSituationId })
                    .HasDatabaseName("IX_Item_UserId_ItemSituationId");
            });
        }

        //migrations
        //no console do gerenciador de pacotes selecione o dal referente:
        //EntityFrameworkCore\Add-Migration "11092026" -Context InventoryDbCtx
        //EntityFrameworkCore\update-database -Context InventoryDbCtx

        //to remove last migration snapshot
        //Remove-Migration -Context InventoryDbCtx 
    }
}