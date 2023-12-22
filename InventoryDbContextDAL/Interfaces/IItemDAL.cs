using InventoryModels;

namespace InventoryDAL.Interfaces
{
    public interface IItemDAL
    {

        int Create(Item item);

        int Delete(Item item);

        int Update(Item item);

        Item? GetById(int uid,int id);
    }
}