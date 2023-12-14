using InventoryModels;

namespace InventoryDAL.Interfaces
{
    public interface IAcquisitionTypeDAL
    {
        List<AcquisitionType>? Get(int uid);
    }
}