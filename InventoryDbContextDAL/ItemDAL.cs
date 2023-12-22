using InventoryDAL.Interfaces;
using InventoryDbContextDAL;
using InventoryModels;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace InventoryDAL
{
    public class ItemDAL(InventoryDbContext dbContext) : IItemDAL
    {
        public Item? GetById(int uid, int id)
            => dbContext.Item.Where(x => x.Id == id && x.UserId == uid)
            .Include(x => x.Category)
            .Include(x => x.SubCategory)
            .Include(x => x.ItemSituation)
            .Include(x => x.AcquisitionType)
            .FirstOrDefault();

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
