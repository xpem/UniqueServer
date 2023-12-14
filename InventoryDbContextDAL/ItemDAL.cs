using InventoryDAL.Interfaces;
using InventoryDbContextDAL;
using InventoryModels;

namespace InventoryDAL
{
    public class ItemDAL(InventoryDbContext dbContext) : IItemDAL
    {
        public int Create(Item item)
        {
            dbContext.Item.Add(item);
            return dbContext.SaveChanges();
        }

        public int Update(Item item)
        {
            dbContext.ChangeTracker?.Clear();
            dbContext.Item.Update(item);
            return dbContext.SaveChanges();
        }

        public int Delete(Item item)
        {
            dbContext.ChangeTracker?.Clear();
            dbContext.Item.Remove(item);
            return dbContext.SaveChanges();
        }
    }
}
