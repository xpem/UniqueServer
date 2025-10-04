using BaseModels;
using InventoryBLL.Interfaces;
using InventoryModels.DTOs;
using InventoryModels.Res;
using InventoryRepos.Interfaces;

namespace InventoryBLL
{
    public class AcquisitionTypeService(IAcquisitionTypeRepo acquisitionTypeDAL) : IAcquisitionTypeService
    {
        public async Task<BaseResponse> Get(int uid)
        {
            List<AcquisitionType>? acquisitionTypes = await acquisitionTypeDAL.Get(uid);

            return new BaseResponse(BuildResAcquisitionType(acquisitionTypes));
        }

        public static List<ResAcquisitionType> BuildResAcquisitionType(List<AcquisitionType>? acquisitionTypes)
        {
            List<ResAcquisitionType> resAcquisitionTypes = [];
            if (acquisitionTypes != null && acquisitionTypes.Count > 0)
                foreach (AcquisitionType acquisitionType in acquisitionTypes)
                    resAcquisitionTypes.Add(
                        new()
                        {
                            Id = acquisitionType.Id,
                            Name = acquisitionType.Name,
                            Sequence = acquisitionType.Sequence,
                            SystemDefault = acquisitionType.SystemDefault,
                        });

            return resAcquisitionTypes;
        }
    }
}
