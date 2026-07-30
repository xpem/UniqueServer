using InventoryModels.DTOs;
using InventoryRepos;
using InventoryRepos.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace InventoryRepo
{
    public class AcquisitionTypeRepo(IDbContextFactory<InventoryDbCtx> dbCtx) : IAcquisitionTypeRepo
    {
        public async Task<List<AcquisitionType>?> Get(int uid)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.AcquisitionType.Where(x => x.UserId == uid || (x.UserId == null && x.SystemDefault)).OrderBy(x => x.Sequence).ToListAsync();
        }
        public async Task<AcquisitionType?> GetById(int uid, int id)
        {
            using var context = dbCtx.CreateDbContext();
            return await context.AcquisitionType.Where(x => (x.UserId == uid || (x.UserId == null && x.SystemDefault)) && x.Id == id).OrderBy(x => x.Sequence).FirstOrDefaultAsync();
        }

    }
}
