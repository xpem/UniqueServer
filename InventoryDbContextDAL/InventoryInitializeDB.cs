﻿using InventoryModels;

namespace InventoryDbContextDAL
{
    public class InventoryInitializeDB()
    {
        public static void CreateInitiaValues(InventoryDbContext inventoryDbContext)
        {
            inventoryDbContext.Database.EnsureCreated();

            CreateBaseCategories(inventoryDbContext);
            CreateBaseSubCategories(inventoryDbContext);
            CreateBaseItemSituation(inventoryDbContext);
            CreateBaseAcquisitionType(inventoryDbContext);

            inventoryDbContext.SaveChanges();
        }

        public static void CreateBaseCategories(InventoryDbContext inventoryDbContext)
        {
            if (inventoryDbContext.Category.Count() is not 0) return;

            InventoryModels.Category[] categories = [
                new Category() { Name = "Casa", Color = "#bfc9ca", SystemDefault = true, CreatedAt = DateTime.Now,Version=0 },
                new Category() { Name = "Vestimenta", Color = "#f5cba7", SystemDefault = true, CreatedAt = DateTime.Now ,Version=0 },
                new Category() { Name = "Carro", Color = "#f5b7b1", SystemDefault = true, CreatedAt = DateTime.Now,Version=0  },
            ];

            inventoryDbContext.Category.AddRange(categories);
        }

        public static void CreateBaseSubCategories(InventoryDbContext inventoryDbContext)
        {
            if (inventoryDbContext.SubCategory.Count() is not 0) return;

            InventoryModels.SubCategory[] subCategories = [
                new SubCategory() { CategoryId = 1, CreatedAt = DateTime.Now, Name = "Móveis", SystemDefault = true, IconName = "Car",Version=0  },
                new SubCategory() { CategoryId = 1, CreatedAt = DateTime.Now, Name = "Eletrodomésticos", SystemDefault = true, IconName = "Tv",Version=0  },
                new SubCategory() { CategoryId = 1, CreatedAt = DateTime.Now, Name = "Computadores", SystemDefault = true, IconName = "Computer" ,Version=0 },
                new SubCategory() { CategoryId = 2, CreatedAt = DateTime.Now, Name = "Eletrônicos", SystemDefault = true, IconName = "Mobile" ,Version=0 },
                new SubCategory() { CategoryId = 2, CreatedAt = DateTime.Now, Name = "Calçados", SystemDefault = true, IconName = "ShoePrints",Version=0  },
                new SubCategory() { CategoryId = 2, CreatedAt = DateTime.Now, Name = "Roupas", SystemDefault = true, IconName = "Tshirt" ,Version=0 },
                new SubCategory() { CategoryId = 3, CreatedAt = DateTime.Now, Name = "Utensílios", SystemDefault = true, IconName = "AirFreshener" ,Version=0 },
                new SubCategory() { CategoryId = 3, CreatedAt = DateTime.Now, Name = "Peças internas", SystemDefault = true, IconName = "Wrench",Version=0  },
                new SubCategory() { CategoryId = 3, CreatedAt = DateTime.Now, Name = "Peças externas", SystemDefault = true, IconName = "Car",Version=0  },
            ];

            inventoryDbContext.SubCategory.AddRange(subCategories);
        }

        public static void CreateBaseItemSituation(InventoryDbContext inventoryDbContext)
        {
            if (inventoryDbContext.ItemSituation.Count() is not 0) return;

            ItemSituation[] itemSituations = [
                new ItemSituation() { Name = "Em uso", CreatedAt = DateTime.Now, SystemDefault = true, Sequence = 1,Type = SituationType.In,Version=0  },
                new ItemSituation() { Name = "Guardado", CreatedAt = DateTime.Now, SystemDefault = true, Sequence = 2,Type = SituationType.In,Version=0  },
                new ItemSituation() { Name = "Dispensado", CreatedAt = DateTime.Now, SystemDefault = true, Sequence = 5,Type=SituationType.Out ,Version=0 },
                new ItemSituation() { Name = "Defeito", CreatedAt = DateTime.Now, SystemDefault = true, Sequence = 3,Type=SituationType.Out,Version=0  },
                new ItemSituation() { Name = "Revendido", CreatedAt = DateTime.Now, SystemDefault = true, Sequence = 4 ,Type = SituationType.Out,Version=0 },
                new ItemSituation() { Name = "Emprestado", CreatedAt = DateTime.Now, SystemDefault = true, Sequence = 6,Type = SituationType.In,Version=0  },
                new ItemSituation() { Name = "Doado", CreatedAt = DateTime.Now, SystemDefault = true, Sequence = 7,Type = SituationType.Out,Version=0  },
            ];

            inventoryDbContext.ItemSituation.AddRange(itemSituations);
        }

        public static void CreateBaseAcquisitionType(InventoryDbContext inventoryDbContext)
        {
            if (inventoryDbContext.AcquisitionType.Count() is not 0) return;

            AcquisitionType[] acquisitionTypes = [
                new AcquisitionType() { Name = "Compra", CreatedAt = DateTime.Now, SystemDefault = true, Sequence = 1,Version=0  },
                new AcquisitionType() { Name = "Emprestimo", CreatedAt = DateTime.Now, SystemDefault = true, Sequence = 2,Version=0  },
                new AcquisitionType() { Name = "Doação", CreatedAt = DateTime.Now, SystemDefault = true, Sequence = 3,Version=0  },
                new AcquisitionType() { Name = "Presente", CreatedAt = DateTime.Now, SystemDefault = true, Sequence = 4,Version=0  },
                new AcquisitionType() { Name = "Troca", CreatedAt = DateTime.Now, SystemDefault = true, Sequence = 5 ,Version=0 },
            ];

            inventoryDbContext.AcquisitionType.AddRange(acquisitionTypes);
        }
    }
}
