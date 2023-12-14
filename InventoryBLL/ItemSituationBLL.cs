using BaseModels;
using InventoryDAL;
using InventoryModels.Res;
using InventoryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryDbContextDAL;
using InventoryDAL.Interfaces;
using InventoryBLL.Interfaces;

namespace InventoryBLL
{
    public class ItemSituationBLL(IItemSituationDAL itemSituationDAL) : IItemSituationBLL
    {
        public BLLResponse Get(int uid)
        {
            List<ItemSituation>? itemSituations = itemSituationDAL.Get(uid);
            List<ResItemSituation> resItemSituations = [];

            if (itemSituations != null && itemSituations.Count > 0)
                foreach (var itemSituation in itemSituations)
                    resItemSituations.Add(
                        new()
                        {
                            Id = itemSituation.Id,
                            Name = itemSituation.Name,
                            Sequence = itemSituation.Sequence,
                            SystemDefault = itemSituation.SystemDefault,
                        });

            return new BLLResponse() { Content = resItemSituations };
        }
    }
}
