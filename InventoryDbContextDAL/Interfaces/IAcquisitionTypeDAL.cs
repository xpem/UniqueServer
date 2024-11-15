using InventoryModels.DTOs;

namespace InventoryRepos.Interfaces
{
    public interface IAcquisitionTypeDAL
    {
        List<AcquisitionType>? Get(int uid);

        AcquisitionType? GetById(int uid, int id);
    }
}