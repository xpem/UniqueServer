﻿using InventoryModels.DTOs;
using Microsoft.EntityFrameworkCore;

namespace InventoryRepos
{
    public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
    {
        //public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        //   : base(options)
        //{
        //    ApplyMigrations(this);
        //}

        public virtual DbSet<Category> Category => Set<Category>();

        public virtual DbSet<SubCategory> SubCategory => Set<SubCategory>();

        public virtual DbSet<ItemSituation> ItemSituation => Set<ItemSituation>();

        public virtual DbSet<AcquisitionType> AcquisitionType => Set<AcquisitionType>();

        public virtual DbSet<Item> Item => Set<Item>();

        //public void ApplyMigrations(InventoryDbContext context)
        //{
        //    if (context.Database.GetPendingMigrations().Any())
        //    {
        //        context.Database.Migrate();
        //    }
        //}

        //migrations
        //no console do gerenciador de pacotes selecione o dal referente:
        //EntityFrameworkCore\Add-Migration "Init" -Context InventoryDbContext
        //EntityFrameworkCore\update-database -Context InventoryDbContext

        //to remove last migration snapshot
        //Remove-Migration -Context InventoryDbContext 
    }
}