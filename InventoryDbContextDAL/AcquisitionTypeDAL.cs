using InventoryDAL.Interfaces;
using InventoryDbContextDAL;
using InventoryModels;

namespace InventoryDAL
{
    public class AcquisitionTypeDAL(InventoryDbContext dbContext) : IAcquisitionTypeDAL
    {
        public List<AcquisitionType>? Get(int uid) => dbContext.AcquisitionType.Where(x => x.UserId == uid || (x.UserId == null && x.SystemDefault)).OrderBy(x => x.Sequence).ToList();

    }
}
