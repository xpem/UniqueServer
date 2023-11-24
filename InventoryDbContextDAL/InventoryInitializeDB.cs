using InventoryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryDbContextDAL
{
    public class InventoryInitializeDB(InventoryDbContext inventoryDbContext)
    {
        public void CreateInitiaValues()
        {
            inventoryDbContext.Database.EnsureCreated();
            CreateBaseCategories();
        }

        public void CreateBaseCategories()
        {
            if (inventoryDbContext.Category.Count() is not 0) return;

            InventoryModels.Category[] categories = [
                new Category() { Name = "Casa", Color = "#bfc9ca", SystemDefault = true, CreatedAt = DateTime.Now },
                new Category() { Name = "Vestimenta", Color = "#f5cba7", SystemDefault = true, CreatedAt = DateTime.Now },
                new Category() { Name = "Carro", Color = "#f5b7b1", SystemDefault = true, CreatedAt = DateTime.Now },
            ];
        }

        //public void CreateBaseSubCategories()
        //{
        //    if (inventoryDbContext.SubCategory.Count() is not 0) return;

        //    InventoryModels.SubCategory[] subCategories = [
        //        new SubCategory() { CategoryId = "Móveis", }
        //    ]
        //}
    }
}
