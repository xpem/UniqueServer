using InventoryModels;

namespace InventoryDbContextDAL
{
    public class InventoryInitializeDB(InventoryDbContext inventoryDbContext)
    {
        public void CreateInitiaValues()
        {
            inventoryDbContext.Database.EnsureCreated();

            CreateBaseCategories();
            CreateBaseSubCategories();

            inventoryDbContext.SaveChanges();
        }

        public void CreateBaseCategories()
        {
            if (inventoryDbContext.Category.Count() is not 0) return;

            InventoryModels.Category[] categories = [
                new Category() { Name = "Casa", Color = "#bfc9ca", SystemDefault = true, CreatedAt = DateTime.Now },
                new Category() { Name = "Vestimenta", Color = "#f5cba7", SystemDefault = true, CreatedAt = DateTime.Now },
                new Category() { Name = "Carro", Color = "#f5b7b1", SystemDefault = true, CreatedAt = DateTime.Now },
            ];

            inventoryDbContext.Category.AddRange(categories);
        }

        public void CreateBaseSubCategories()
        {
            if (inventoryDbContext.SubCategory.Count() is not 0) return;

            InventoryModels.SubCategory[] subCategories = [
                new SubCategory() { CategoryId = 1, CreatedAt = DateTime.Now, Name = "Móveis", SystemDefault = true, IconName = "Car" },
                new SubCategory() { CategoryId = 1, CreatedAt = DateTime.Now, Name = "Eletrodomésticos", SystemDefault = true, IconName = "Tv" },
                new SubCategory() { CategoryId = 1, CreatedAt = DateTime.Now, Name = "Computadores", SystemDefault = true, IconName = "Computer" },
                new SubCategory() { CategoryId = 2, CreatedAt = DateTime.Now, Name = "Eletrônicos", SystemDefault = true, IconName = "Mobile" },
                new SubCategory() { CategoryId = 2, CreatedAt = DateTime.Now, Name = "Calçados", SystemDefault = true, IconName = "ShoePrints" },
                new SubCategory() { CategoryId = 2, CreatedAt = DateTime.Now, Name = "Roupas", SystemDefault = true, IconName = "Tshirt" },
                new SubCategory() { CategoryId = 3, CreatedAt = DateTime.Now, Name = "Utensílios", SystemDefault = true, IconName = "AirFreshener" },
                new SubCategory() { CategoryId = 3, CreatedAt = DateTime.Now, Name = "Peças internas", SystemDefault = true, IconName = "Wrench" },
                new SubCategory() { CategoryId = 3, CreatedAt = DateTime.Now, Name = "Peças externas", SystemDefault = true, IconName = "Car" },
            ];

            inventoryDbContext.SubCategory.AddRange(subCategories);
        }
    }
}
