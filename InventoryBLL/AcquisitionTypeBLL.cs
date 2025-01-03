﻿using BaseModels;
using InventoryBLL.Interfaces;
using InventoryModels.DTOs;
using InventoryModels.Res;
using InventoryRepos.Interfaces;

namespace InventoryBLL
{
    public class AcquisitionTypeBLL(IAcquisitionTypeDAL acquisitionTypeDAL) : IAcquisitionTypeBLL
    {
        public BaseResponse Get(int uid)
        {
            List<AcquisitionType>? acquisitionTypes = acquisitionTypeDAL.Get(uid);
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

            return new BaseResponse(resAcquisitionTypes);
        }
    }
}
