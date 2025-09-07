using InventoryModels.DTOs;
using InventoryRepos.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace InventoryRepos
{
    public class AcquisitionTypeRepo(InventoryDbContext dbContext) : IAcquisitionTypeRepo
    {
        public async Task<List<AcquisitionType>?> Get(int uid) => await dbContext.AcquisitionType.Where(x => x.UserId == uid || (x.UserId == null && x.SystemDefault)).OrderBy(x => x.Sequence).ToListAsync();
        public async Task<AcquisitionType?> GetById(int uid, int id) =>await  dbContext.AcquisitionType.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Id == id).OrderBy(x => x.Sequence).FirstOrDefaultAsync();

    }
}
