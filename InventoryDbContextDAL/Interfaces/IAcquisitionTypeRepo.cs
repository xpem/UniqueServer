using InventoryModels.DTOs;

namespace InventoryRepos.Interfaces
{
    public interface IAcquisitionTypeRepo
    {
        Task<List<AcquisitionType>?> Get(int uid);

        Task<AcquisitionType?> GetById(int uid, int id);
    }
}