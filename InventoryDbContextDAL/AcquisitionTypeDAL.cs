using InventoryModels.DTOs;
using InventoryRepos.Interfaces;

namespace InventoryRepos
{
    public class AcquisitionTypeDAL(InventoryDbContext dbContext) : IAcquisitionTypeDAL
    {
        public List<AcquisitionType>? Get(int uid) => dbContext.AcquisitionType.Where(x => x.UserId == uid || (x.UserId == null && x.SystemDefault)).OrderBy(x => x.Sequence).ToList();
        public AcquisitionType? GetById(int uid, int id) => dbContext.AcquisitionType.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Id == id).OrderBy(x => x.Sequence).FirstOrDefault();

    }
}
